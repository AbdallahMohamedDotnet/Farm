using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebApplication2.Services;
using System.Net;

namespace WebApplication2.Middleware
{
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var securityService = scope.ServiceProvider.GetRequiredService<ISecurityService>();
                
                // Get client IP for rate limiting
                var clientIP = GetClientIP(context.Request);
                
                // Check rate limiting
                if (!await securityService.CheckRateLimit(clientIP))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
                    return;
                }
                
                // Validate request security
                if (!await securityService.ValidateRequestSecurity(context.Request))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("Security validation failed.");
                    return;
                }
                
                // Add security headers
                AddSecurityHeaders(context.Response);
                
                // Validate user session if authenticated
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    if (!await securityService.ValidateUserSession(context.User))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await context.Response.WriteAsync("Invalid session.");
                        return;
                    }
                }
                
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Security middleware error");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("An error occurred while processing your request.");
            }
        }

        private void AddSecurityHeaders(HttpResponse response)
        {
            // Prevent clickjacking
            response.Headers["X-Frame-Options"] = "DENY";
            
            // Prevent MIME type sniffing
            response.Headers["X-Content-Type-Options"] = "nosniff";
            
            // Enable XSS protection
            response.Headers["X-XSS-Protection"] = "1; mode=block";
            
            // Strict transport security (HTTPS only)
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            
            // Content Security Policy
            response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'; frame-ancestors 'none';";
            
            // Referrer Policy
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            
            // Permissions Policy
            response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            
            // Remove server header
            response.Headers.Remove("Server");
        }

        private string GetClientIP(HttpRequest request)
        {
            return request.Headers["X-Forwarded-For"].FirstOrDefault() ??
                   request.Headers["X-Real-IP"].FirstOrDefault() ??
                   request.HttpContext.Connection.RemoteIpAddress?.ToString() ??
                   "Unknown";
        }
    }
}