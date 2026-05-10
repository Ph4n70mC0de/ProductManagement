using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface IAuditLogRepository : IGenericRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50);
        Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityName, int entityId);
    }
}