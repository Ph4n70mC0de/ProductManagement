using System.ComponentModel.DataAnnotations;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, ILogger<UserService> logger)
        {
            _repository = repository;
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

            try
            {
                user.CreatedAt = DateTime.UtcNow;
                user.IsDeleted = false;
                return await _repository.AddAsync(user);
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

            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(user);
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
                    user.IsDeleted = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user with id {UserId}", id);
                throw new ServiceException($"Failed to delete user with id {id}", ex);
            }
        }

        private void ValidateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ValidationException("Username is required");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ValidationException("Email is required");

            if (!IsValidEmail(user.Email))
                throw new ValidationException("Invalid email format");

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ValidationException("Password is required for new users");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}