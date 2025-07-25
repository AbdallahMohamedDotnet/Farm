namespace WebApplication2.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, string entityName, string? entityId = null, string? details = null);
    }
}