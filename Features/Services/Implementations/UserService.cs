using System.Text.Json;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, IAuditLogService auditLogService, ILogger<UserService> logger)
        {
            _repository = repository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve users");
                return Enumerable.Empty<User>();
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve user with id {UserId}", id);
                return null;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            ValidateUser(user);
            HashPassword(user);

            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.IsDeleted = false;
                var result = await _repository.AddAsync(user);
                await _auditLogService.LogActionAsync("User", result.Id, "Create", null, JsonSerializer.Serialize(new { result.Username, result.Email }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                throw new ServiceException("Failed to create user", ex);
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            ValidateUser(user);
            HashPasswordIfNeeded(user);

            try
            {
                var oldUser = await _repository.GetByIdAsync(user.Id);
                user.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(user);
                await _auditLogService.LogActionAsync("User", user.Id, "Update",
                    oldUser != null ? JsonSerializer.Serialize(new { oldUser.Username, oldUser.Email }) : null,
                    JsonSerializer.Serialize(new { result.Username, result.Email }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user with id {UserId}", user.Id);
                throw new ServiceException($"Failed to update user with id {user.Id}", ex);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var user = await _repository.GetByIdAsync(id);
                if (user != null)
                {
                    var oldValues = JsonSerializer.Serialize(new { user.Username, user.Email, user.IsDeleted });
                    user.IsDeleted = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(user);
                    await _auditLogService.LogActionAsync("User", id, "Delete", oldValues, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with id {UserId}", id);
                throw new ServiceException($"Failed to delete user with id {id}", ex);
            }
        }

        private static void ValidateUser(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            ValidationHelper.ValidateRequiredString(user.Username, "Username");
            ValidationHelper.ValidateEmail(user.Email, "Email");
            ValidationHelper.ValidateRequiredString(user.PasswordHash, "Password");
        }

        private static void HashPassword(User user)
        {
            if (!IsPasswordHashed(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }
        }

        private static void HashPasswordIfNeeded(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.PasswordHash) && !IsPasswordHashed(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }
        }

        private static bool IsPasswordHashed(string password)
        {
            return password.StartsWith("$2b$") || password.StartsWith("$2a$") || password.StartsWith("$2y$");
        }
    }
}