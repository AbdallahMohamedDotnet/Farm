using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebApplication2.Services;

namespace WebApplication2.Middleware
{
    public class TokenDecryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenDecryptionMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TokenDecryptionMiddleware(RequestDelegate next, ILogger<TokenDecryptionMiddleware> logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check if request has Authorization header with Bearer token
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var encryptedToken = authHeader.Substring("Bearer ".Length).Trim();
                    
                    if (!string.IsNullOrEmpty(encryptedToken))
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var tokenEncryption = scope.ServiceProvider.GetRequiredService<ITokenEncryptionService>();
                        
                        try
                        {
                            // Decrypt the token
                            var decryptedToken = tokenEncryption.DecryptToken(encryptedToken);
                            
                            // Validate token integrity
                            if (tokenEncryption.ValidateTokenIntegrity(decryptedToken))
                            {
                                // Replace the encrypted token with the decrypted one
                                context.Request.Headers["Authorization"] = $"Bearer {decryptedToken}";
                                _logger.LogDebug("Token decrypted successfully");
                            }
                            else
                            {
                                _logger.LogWarning("Token integrity validation failed");
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("Invalid token format");
                                return;
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            _logger.LogWarning(ex, "Token decryption failed");
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Invalid token");
                            return;
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token decryption middleware error");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An error occurred while processing your request.");
            }
        }
    }
}