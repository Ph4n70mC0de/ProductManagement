using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50);
        Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityName, int entityId);
        Task LogActionAsync(string entityName, int entityId, string action, string? oldValues = null, string? newValues = null, string? userName = null);
    }
}