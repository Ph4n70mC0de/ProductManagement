using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly AppAuthenticationStateProvider _authStateProvider;

        public AuthenticationService(
            AppDbContext context, 
            ILogger<AuthenticationService> logger,
            AppAuthenticationStateProvider authStateProvider)
        {
            _context = context;
            _logger = logger;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthenticationResult> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted && u.IsActive);

                if (user == null)
                {
                    return new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password" };
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    return new AuthenticationResult { Success = false, ErrorMessage = "Invalid username or password" };
                }

                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _authStateProvider.MarkUserAsAuthenticated(user);

                return new AuthenticationResult { Success = true, User = user };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user {Username}", username);
                return new AuthenticationResult { Success = false, ErrorMessage = "An error occurred during login" };
            }
        }

        public async Task LogoutAsync()
        {
            await _authStateProvider.MarkUserAsLoggedOut();
        }
    }
}