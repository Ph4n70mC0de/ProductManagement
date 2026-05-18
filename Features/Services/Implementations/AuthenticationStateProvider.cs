using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Services.Interfaces;
using System.Security.Claims;

namespace ProductManagement.Features.Services.Implementations
{
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        private User? _currentUser;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;

            if (_currentUser != null)
            {
                identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, _currentUser.Username),
                    new Claim(ClaimTypes.Email, _currentUser.Email),
                    new Claim(ClaimTypes.Role, _currentUser.Role?.Name ?? "User"),
                }, "Server authentication");
            }
            else if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                identity = new ClaimsIdentity(_httpContextAccessor.HttpContext.User.Claims, "Server authentication");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(User user)
        {
            _currentUser = user;
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
            }, "Server authentication"));

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));
        }

        public async Task MarkUserAsLoggedOut()
        {
            _currentUser = null;
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
        }

        public User? GetCurrentUser() => _currentUser;
    }
}