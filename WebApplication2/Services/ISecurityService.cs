using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WebApplication2.Services
{
    public interface ISecurityService
    {
        Task<bool> ValidateRequestSecurity(HttpRequest request);
        Task<bool> ValidateUserSession(ClaimsPrincipal user);
        Task LogSecurityEvent(string eventType, string details, string? userId = null);
        bool ValidateApiKey(string apiKey);
        string GenerateApiKey();
        Task<bool> CheckRateLimit(string identifier);
        Task<bool> ValidateCSRFToken(string token);
    }
}