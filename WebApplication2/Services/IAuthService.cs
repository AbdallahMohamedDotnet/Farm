using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
        Task<AuthResponseDto> ResendOtpAsync(ResendOtpDto resendOtpDto);
        Task<AuthResponseDto> GetTokenAsync(LoginDto loginDto); // New method for getting JWT token
    }
}