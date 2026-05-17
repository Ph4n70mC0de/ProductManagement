using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Services.Interfaces;
using System.Security.Claims;

namespace ProductManagement.Features.Services.Implementations
{
    public class AuthenticationService : ProductManagement.Features.Services.Interfaces.IAuthenticationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly AppAuthenticationStateProvider _authStateProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(
            AppDbContext context, 
            ILogger<AuthenticationService> logger,
            AppAuthenticationStateProvider authStateProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _authStateProvider = authStateProvider;
            _httpContextAccessor = httpContextAccessor;
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

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                if (_httpContextAccessor.HttpContext != null)
                {
                    await _httpContextAccessor.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                }

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
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}