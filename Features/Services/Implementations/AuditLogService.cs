using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 50)
        {
            try
            {
                return await _repository.GetRecentLogsAsync(count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve recent audit logs");
                return Enumerable.Empty<AuditLog>();
            }
        }

        public async Task<IEnumerable<AuditLog>> GetLogsByEntityAsync(string entityName, int entityId)
        {
            try
            {
                return await _repository.GetLogsByEntityAsync(entityName, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve audit logs for entity {EntityName} with id {EntityId}", entityName, entityId);
                return Enumerable.Empty<AuditLog>();
            }
        }

        public async Task LogActionAsync(string entityName, int entityId, string action, string? oldValues = null, string? newValues = null, string? userName = null)
        {
            try
            {
                var log = new AuditLog
                {
                    EntityName = entityName,
                    EntityId = entityId,
                    Action = action,
                    OldValues = oldValues,
                    NewValues = newValues,
                    UserName = userName,
                    CreatedAt = DateTime.UtcNow
                };
                await _repository.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit action {Action} for entity {EntityName} with id {EntityId}", action, entityName, entityId);
                throw new ServiceException($"Failed to log audit action for entity {entityName} with id {entityId}", ex);
            }
        }
    }
}