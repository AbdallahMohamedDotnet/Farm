using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ITokenEncryptionService _tokenEncryption;
        private readonly ISecurityService _securityService;
        
        public AuthService(
            UserManager<User> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context, 
            IConfiguration configuration,
            IEmailService emailService,
            IAuditService auditService,
            ITokenEncryptionService tokenEncryption,
            ISecurityService securityService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _auditService = auditService;
            _tokenEncryption = tokenEncryption;
            _securityService = securityService;
        }
        
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                await _securityService.LogSecurityEvent("DuplicateRegistration", $"Attempted duplicate registration for: {registerDto.Email}");
                return new AuthResponseDto { Message = "User already exists", Success = false };
            }
            
            // Check if there's already a pending registration for this email
            var existingPending = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == registerDto.Email && !p.IsConfirmed);
            
            if (existingPending != null)
            {
                // Remove expired pending registration
                if (existingPending.ExpiresAt < DateTime.UtcNow)
                {
                    _context.PendingRegistrations.Remove(existingPending);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return new AuthResponseDto 
                    { 
                        Message = "A registration for this email is already pending. Please check your email for OTP or wait for it to expire.",
                        Success = false 
                    };
                }
            }
            
            // Hash the password securely
            var passwordHasher = new PasswordHasher<User>();
            var passwordHash = passwordHasher.HashPassword(null!, registerDto.Password);
            
            // Create pending registration
            var pendingRegistration = new PendingRegistration
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                Username = registerDto.Username,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30), // 30 minutes to confirm
                IsConfirmed = false
            };
            
            _context.PendingRegistrations.Add(pendingRegistration);
            await _context.SaveChangesAsync();
            
            // Generate and send OTP for email confirmation
            var otpCode = await _emailService.GenerateOtpAsync(registerDto.Email, "EmailConfirmation");
            await _emailService.SendOtpEmailAsync(registerDto.Email, otpCode, "EmailConfirmation");
            
            await _auditService.LogActionAsync("System", "RegisterInitiated", "PendingRegistration", pendingRegistration.Id.ToString(), 
                $"Registration initiated for: {registerDto.Email}");
            
            return new AuthResponseDto 
            { 
                Message = "Registration initiated. Please check your email for the verification code to complete your registration.", 
                Success = true,
                RequiresEmailConfirmation = true
            };
        }
        
        public async Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            // Validate OTP
            var isValidOtp = await _emailService.ValidateOtpAsync(confirmEmailDto.Email, confirmEmailDto.OtpCode, "EmailConfirmation");
            if (!isValidOtp)
            {
                await _securityService.LogSecurityEvent("InvalidOTP", $"Invalid OTP attempt for: {confirmEmailDto.Email}");
                return new AuthResponseDto { Message = "Invalid or expired OTP", Success = false };
            }
            
            // Find pending registration
            var pendingRegistration = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == confirmEmailDto.Email && !p.IsConfirmed);
            
            if (pendingRegistration == null)
            {
                return new AuthResponseDto { Message = "No pending registration found for this email", Success = false };
            }
            
            if (pendingRegistration.ExpiresAt < DateTime.UtcNow)
            {
                _context.PendingRegistrations.Remove(pendingRegistration);
                await _context.SaveChangesAsync();
                return new AuthResponseDto { Message = "Registration has expired. Please register again.", Success = false };
            }
            
            // Create the actual user
            var user = new User
            {
                UserName = pendingRegistration.Username,
                Email = pendingRegistration.Email,
                FirstName = pendingRegistration.FirstName,
                LastName = pendingRegistration.LastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            // Create user with the hashed password
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return new AuthResponseDto 
                { 
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)),
                    Success = false
                };
            }
            
            // Set the password hash manually
            user.PasswordHash = pendingRegistration.PasswordHash;
            await _userManager.UpdateAsync(user);
            
            // Assign default role (Customer)
            await _userManager.AddToRoleAsync(user, UserRoles.Customer);
            
            // Create farm for the user
            var farm = new Farm
            {
                Id = Guid.NewGuid(),
                Name = $"{pendingRegistration.FirstName} {pendingRegistration.LastName}'s Farm",
                OwnerId = user.Id
            };
            
            _context.Farms.Add(farm);
            
            // Mark pending registration as confirmed and remove it
            _context.PendingRegistrations.Remove(pendingRegistration);
            
            await _context.SaveChangesAsync();
            
            // Log the registration completion
            await _auditService.LogActionAsync(user.Id, "EmailConfirmed", "User", user.Id, "Email confirmed and user created successfully");
            
            return new AuthResponseDto 
            { 
                Message = "Email confirmed successfully! Your account has been created.", 
                Success = true,
                UserName = $"{user.FirstName} {user.LastName}"
            };
        }
        
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _securityService.LogSecurityEvent("FailedLogin", $"Failed login attempt for: {loginDto.Email}");
                return new AuthResponseDto { Message = "Invalid email or password", Success = false };
            }
            
            if (!user.IsActive)
            {
                await _securityService.LogSecurityEvent("InactiveUserLogin", $"Inactive user login attempt: {loginDto.Email}");
                return new AuthResponseDto { Message = "Account is deactivated", Success = false };
            }
            
            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto 
                { 
                    Message = "Email not confirmed. Please confirm your email first.", 
                    Success = false,
                    RequiresEmailConfirmation = true
                };
            }
            
            // Log the successful login
            await _auditService.LogActionAsync(user.Id, "Login", "User", user.Id, "User logged in successfully");
            
            // Return only the user's full name (no token for basic login)
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                fullName = user.UserName!;
            }
            
            return new AuthResponseDto 
            { 
                Message = "Login successful", 
                Success = true,
                UserName = fullName
            };
        }
        
        public async Task<AuthResponseDto> GetTokenAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                await _securityService.LogSecurityEvent("FailedTokenRequest", $"Failed token request for: {loginDto.Email}");
                return new AuthResponseDto { Message = "Invalid email or password", Success = false };
            }
            
            if (!user.IsActive)
            {
                await _securityService.LogSecurityEvent("InactiveUserTokenRequest", $"Inactive user token request: {loginDto.Email}");
                return new AuthResponseDto { Message = "Account is deactivated", Success = false };
            }
            
            if (!user.EmailConfirmed)
            {
                return new AuthResponseDto 
                { 
                    Message = "Email not confirmed. Please confirm your email first.", 
                    Success = false,
                    RequiresEmailConfirmation = true
                };
            }
            
            // Generate JWT token
            var jwtToken = await GenerateJwtTokenAsync(user);
            
            // Encrypt the token
            var encryptedToken = _tokenEncryption.EncryptToken(jwtToken);
            
            // Log the token generation
            await _auditService.LogActionAsync(user.Id, "TokenGenerated", "User", user.Id, "JWT token generated successfully");
            
            return new AuthResponseDto 
            { 
                Message = "Token generated successfully", 
                Token = encryptedToken,
                Success = true,
                UserName = $"{user.FirstName} {user.LastName}".Trim()
            };
        }
        
        public async Task<AuthResponseDto> ResendOtpAsync(ResendOtpDto resendOtpDto)
        {
            // Check if there's a pending registration
            var pendingRegistration = await _context.PendingRegistrations
                .FirstOrDefaultAsync(p => p.Email == resendOtpDto.Email && !p.IsConfirmed);
            
            if (pendingRegistration == null)
            {
                // Check if user exists but not confirmed
                var user = await _userManager.FindByEmailAsync(resendOtpDto.Email);
                if (user == null)
                {
                    return new AuthResponseDto { Message = "No registration found for this email", Success = false };
                }
            }
            else
            {
                // Check if pending registration has expired
                if (pendingRegistration.ExpiresAt < DateTime.UtcNow)
                {
                    _context.PendingRegistrations.Remove(pendingRegistration);
                    await _context.SaveChangesAsync();
                    return new AuthResponseDto { Message = "Registration has expired. Please register again.", Success = false };
                }
            }
            
            var otpCode = await _emailService.GenerateOtpAsync(resendOtpDto.Email, resendOtpDto.Purpose);
            await _emailService.SendOtpEmailAsync(resendOtpDto.Email, otpCode, resendOtpDto.Purpose);
            
            await _auditService.LogActionAsync("System", "OTPResent", "Email", null, $"OTP resent to: {resendOtpDto.Email}");
            
            return new AuthResponseDto { Message = "OTP sent successfully", Success = true };
        }
        
        private async Task<string> GenerateJwtTokenAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);
            
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? ""),
                new Claim("jti", Guid.NewGuid().ToString()), // Unique token ID
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Issued at
            };
            
            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpirationInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}