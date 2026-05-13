using System.Text.Json;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<RoleService> _logger;

        public RoleService(IRoleRepository repository, IAuditLogService auditLogService, ILogger<RoleService> logger)
        {
            _repository = repository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve roles");
                return Enumerable.Empty<Role>();
            }
        }

        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve role with id {RoleId}", id);
                return null;
            }
        }

        public async Task<Role> CreateRoleAsync(Role role)
        {
            ValidationHelper.ValidateRequiredString(role.Name, "Role name");

            try
            {
                role.CreatedAt = DateTime.UtcNow;
                role.IsDeleted = false;
                var result = await _repository.AddAsync(role);
                await _auditLogService.LogActionAsync("Role", result.Id, "Create", null, 
                    JsonSerializer.Serialize(new { result.Name, result.Description }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create role");
                throw new ServiceException("Failed to create role", ex);
            }
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            ValidationHelper.ValidateRequiredString(role.Name, "Role name");

            try
            {
                var oldRole = await _repository.GetByIdAsync(role.Id);
                role.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(role);
                await _auditLogService.LogActionAsync("Role", role.Id, "Update",
                    oldRole != null ? JsonSerializer.Serialize(new { oldRole.Name, oldRole.Description }) : null,
                    JsonSerializer.Serialize(new { result.Name, result.Description }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update role with id {RoleId}", role.Id);
                throw new ServiceException($"Failed to update role with id {role.Id}", ex);
            }
        }

        public async Task DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _repository.GetByIdAsync(id);
                if (role != null)
                {
                    var oldValues = JsonSerializer.Serialize(new { role.Name, role.Description, role.IsDeleted });
                    role.IsDeleted = true;
                    role.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(role);
                    await _auditLogService.LogActionAsync("Role", id, "Delete", oldValues, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete role with id {RoleId}", id);
                throw new ServiceException($"Failed to delete role with id {id}", ex);
            }
        }
    }
}