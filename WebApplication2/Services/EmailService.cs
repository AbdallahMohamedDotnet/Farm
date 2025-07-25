using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;
        
        public EmailService(IConfiguration configuration, ApplicationDbContext context, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }
        
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                
                using var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]!))
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(emailSettings["SenderEmail"], emailSettings["SenderPassword"])
                };
                
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailSettings["SenderEmail"]!, emailSettings["SenderName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);
                
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }
        
        public async Task<bool> SendOtpEmailAsync(string email, string otpCode, string purpose)
        {
            var subject = purpose switch
            {
                "EmailConfirmation" => "Confirm Your Email - Farm Management System",
                "PasswordReset" => "Reset Your Password - Farm Management System",
                _ => "Verification Code - Farm Management System"
            };
            
            var body = purpose switch
            {
                "EmailConfirmation" => $@"
                    <h2>Welcome to Farm Management System!</h2>
                    <p>Please confirm your email address by using the following verification code:</p>
                    <h3 style='color: #2E7D32; font-size: 24px; letter-spacing: 3px;'>{otpCode}</h3>
                    <p>This code will expire in 15 minutes.</p>
                    <p>If you didn't create an account, please ignore this email.</p>",
                "PasswordReset" => $@"
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password. Use the following verification code:</p>
                    <h3 style='color: #D32F2F; font-size: 24px; letter-spacing: 3px;'>{otpCode}</h3>
                    <p>This code will expire in 15 minutes.</p>
                    <p>If you didn't request this, please ignore this email.</p>",
                _ => $@"
                    <h2>Verification Code</h2>
                    <p>Your verification code is:</p>
                    <h3 style='color: #1976D2; font-size: 24px; letter-spacing: 3px;'>{otpCode}</h3>
                    <p>This code will expire in 15 minutes.</p>"
            };
            
            return await SendEmailAsync(email, subject, body);
        }
        
        public async Task<string> GenerateOtpAsync(string email, string purpose)
        {
            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            
            // For pending registrations, we'll store OTP with a temporary user identifier
            var tempUserId = $"pending_{email}_{purpose}";
            
            // Remove any existing OTPs for this email and purpose (for pending registrations)
            var existingOtps = await _context.UserOtps
                .Where(o => o.UserId == tempUserId && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();
            
            _context.UserOtps.RemoveRange(existingOtps);
            
            // Also remove existing OTPs for confirmed users with this email
            var existingUserOtps = await _context.UserOtps
                .Include(o => o.User)
                .Where(o => o.User != null && o.User.Email == email && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();
            
            _context.UserOtps.RemoveRange(existingUserOtps);
            
            // Create new OTP
            var otp = new UserOtp
            {
                Id = Guid.NewGuid(),
                UserId = tempUserId,
                OtpCode = otpCode,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes expiry
                IsUsed = false
            };
            
            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();
            
            return otpCode;
        }
        
        public async Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose)
        {
            var tempUserId = $"pending_{email}_{purpose}";
            
            var otp = await _context.UserOtps
                .FirstOrDefaultAsync(o => o.UserId == tempUserId 
                                    && o.OtpCode == otpCode 
                                    && o.Purpose == purpose 
                                    && !o.IsUsed 
                                    && o.ExpiresAt > DateTime.UtcNow);
            
            if (otp == null)
                return false;
            
            // Mark OTP as used
            otp.IsUsed = true;
            await _context.SaveChangesAsync();
            
            return true;
        }
        
        // New method for generating OTP for existing users
        public async Task<string> GenerateOtpForUserAsync(string userId, string purpose)
        {
            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            
            // Remove any existing OTPs for this user and purpose
            var existingOtps = await _context.UserOtps
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();
            
            _context.UserOtps.RemoveRange(existingOtps);
            
            // Create new OTP
            var otp = new UserOtp
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OtpCode = otpCode,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes expiry
                IsUsed = false
            };
            
            _context.UserOtps.Add(otp);
            await _context.SaveChangesAsync();
            
            return otpCode;
        }
    }
}