using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecurityService> _logger;
        private readonly IAuditService _auditService;

        public SecurityService(
            ApplicationDbContext context,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<SecurityService> logger,
            IAuditService auditService)
        {
            _context = context;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<bool> ValidateRequestSecurity(HttpRequest request)
        {
            try
            {
                // Validate HTTPS
                if (!request.IsHttps && !IsLocalDevelopment())
                {
                    _logger.LogWarning("Non-HTTPS request rejected from {IP}", GetClientIP(request));
                    return false;
                }

                // Validate headers
                if (!ValidateSecurityHeaders(request))
                {
                    _logger.LogWarning("Invalid security headers from {IP}", GetClientIP(request));
                    return false;
                }

                // Check for suspicious patterns
                if (await DetectSuspiciousActivity(request))
                {
                    _logger.LogWarning("Suspicious activity detected from {IP}", GetClientIP(request));
                    await LogSecurityEvent("SuspiciousActivity", $"Suspicious request from {GetClientIP(request)}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating request security");
                return false;
            }
        }

        public async Task<bool> ValidateUserSession(ClaimsPrincipal user)
        {
            try
            {
                if (!user.Identity?.IsAuthenticated == true)
                    return false;

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return false;

                // Check if user is still active
                var dbUser = await _context.Users.FindAsync(userId);
                if (dbUser == null || !dbUser.IsActive)
                {
                    await LogSecurityEvent("InvalidSession", $"Inactive user attempted access: {userId}", userId);
                    return false;
                }

                // Validate session timing
                var tokenExp = user.FindFirst("exp")?.Value;
                if (!string.IsNullOrEmpty(tokenExp))
                {
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(tokenExp));
                    if (expTime < DateTimeOffset.UtcNow)
                    {
                        await LogSecurityEvent("ExpiredToken", $"Expired token used by user: {userId}", userId);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user session");
                return false;
            }
        }

        public async Task LogSecurityEvent(string eventType, string details, string? userId = null)
        {
            try
            {
                await _auditService.LogActionAsync(
                    userId ?? "System",
                    eventType,
                    "Security",
                    userId,
                    details
                );

                _logger.LogWarning("Security Event: {EventType} - {Details}", eventType, details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log security event");
            }
        }

        public bool ValidateApiKey(string apiKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    return false;

                var validApiKeys = _configuration.GetSection("SecuritySettings:ApiKeys").Get<string[]>() ?? Array.Empty<string>();
                
                // Use secure comparison to prevent timing attacks
                return validApiKeys.Any(key => SecureEquals(apiKey, key));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return false;
            }
        }

        public string GenerateApiKey()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> CheckRateLimit(string identifier)
        {
            try
            {
                var key = $"rate_limit_{identifier}";
                var limit = _configuration.GetValue<int>("SecuritySettings:RateLimit:RequestsPerMinute", 100);
                var window = TimeSpan.FromMinutes(1);

                if (_cache.TryGetValue(key, out int currentCount))
                {
                    if (currentCount >= limit)
                    {
                        await LogSecurityEvent("RateLimitExceeded", $"Rate limit exceeded for: {identifier}");
                        return false;
                    }
                    _cache.Set(key, currentCount + 1, window);
                }
                else
                {
                    _cache.Set(key, 1, window);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit");
                return true; // Allow request on error to prevent blocking legitimate users
            }
        }

        public Task<bool> ValidateCSRFToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Task.FromResult(false);

                // Implement CSRF token validation logic
                var storedToken = _cache.Get<string>($"csrf_{token}");
                if (storedToken == null)
                {
                    _ = LogSecurityEvent("InvalidCSRFToken", $"Invalid CSRF token: {token}");
                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CSRF token");
                return Task.FromResult(false);
            }
        }

        private bool ValidateSecurityHeaders(HttpRequest request)
        {
            // Check for required security headers
            var requiredHeaders = new[]
            {
                "User-Agent",
                "Accept"
            };

            foreach (var header in requiredHeaders)
            {
                if (!request.Headers.ContainsKey(header))
                {
                    return false;
                }
            }

            // Validate User-Agent is not suspicious
            var userAgent = request.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(userAgent) || IsSuspiciousUserAgent(userAgent))
            {
                return false;
            }

            return true;
        }

        private async Task<bool> DetectSuspiciousActivity(HttpRequest request)
        {
            var clientIP = GetClientIP(request);
            var path = request.Path.Value?.ToLower() ?? "";

            // Check for common attack patterns
            var suspiciousPatterns = new[]
            {
                "script", "select", "union", "drop", "delete", "insert",
                "../", "..\\", "<script>", "javascript:", "vbscript:",
                "onload=", "onerror=", "eval(", "alert("
            };

            var queryString = request.QueryString.Value?.ToLower() ?? "";
            var combinedInput = $"{path} {queryString}";

            if (suspiciousPatterns.Any(pattern => combinedInput.Contains(pattern)))
            {
                return true;
            }

            // Check request frequency from same IP
            var rateLimitKey = $"ip_requests_{clientIP}";
            if (_cache.TryGetValue(rateLimitKey, out int requestCount))
            {
                if (requestCount > 500) // 500 requests per minute threshold
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSuspiciousUserAgent(string userAgent)
        {
            var suspiciousAgents = new[]
            {
                "sqlmap", "nikto", "burp", "nessus", "openvas",
                "wget", "curl", "python-requests", "bot", "crawler"
            };

            return suspiciousAgents.Any(agent => 
                userAgent.ToLower().Contains(agent));
        }

        private string GetClientIP(HttpRequest request)
        {
            return request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                   request.Headers["X-Real-IP"].FirstOrDefault() ??
                   request.HttpContext.Connection.RemoteIpAddress?.ToString() ??
                   "Unknown";
        }

        private bool IsLocalDevelopment()
        {
            return _configuration.GetValue<bool>("Development:AllowHttp", false);
        }

        private static bool SecureEquals(string a, string b)
        {
            if (a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }
            return result == 0;
        }
    }
}