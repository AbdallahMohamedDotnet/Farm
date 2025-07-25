using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models.DTOs;
using WebApplication2.Services;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        /// <summary>
        /// Step 1: Register user - Collects name, email, password and sends OTP
        /// User is NOT created in database yet
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.RegisterAsync(registerDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Step 2: Confirm email with OTP - Creates user in database and farm after successful validation
        /// </summary>
        [HttpPost("confirm-email")]
        public async Task<ActionResult<AuthResponseDto>> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Step 3: Login with email and password - Returns only user's name (no token)
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.Success)
            {
                if (result.RequiresEmailConfirmation)
                {
                    return BadRequest(result);
                }
                return Unauthorized(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Get encrypted JWT token for role-based access to protected endpoints
        /// </summary>
        [HttpPost("get-token")]
        public async Task<ActionResult<AuthResponseDto>> GetToken([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.GetTokenAsync(loginDto);
            
            if (!result.Success)
            {
                if (result.RequiresEmailConfirmation)
                {
                    return BadRequest(result);
                }
                return Unauthorized(result);
            }
            
            return Ok(result);
        }
        
        /// <summary>
        /// Resend OTP for email confirmation
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<ActionResult<AuthResponseDto>> ResendOtp([FromBody] ResendOtpDto resendOtpDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _authService.ResendOtpAsync(resendOtpDto);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
}