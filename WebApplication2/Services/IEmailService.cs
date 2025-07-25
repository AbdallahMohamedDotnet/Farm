using WebApplication2.Models.DTOs;

namespace WebApplication2.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task<bool> SendOtpEmailAsync(string email, string otpCode, string purpose);
        Task<string> GenerateOtpAsync(string email, string purpose);
        Task<bool> ValidateOtpAsync(string email, string otpCode, string purpose);
        Task<string> GenerateOtpForUserAsync(string userId, string purpose);
    }
}