using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;
        
        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task LogActionAsync(string userId, string action, string entityName, string? entityId = null, string? details = null)
        {
            try
            {
                Guid? parsedEntityId = null;
                if (!string.IsNullOrEmpty(entityId) && Guid.TryParse(entityId, out var guid))
                {
                    parsedEntityId = guid;
                }
                
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = parsedEntityId,
                    Details = details,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Audit log created: {action} on {entityName} by user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create audit log for action {action} on {entityName} by user {userId}");
            }
        }
    }
}