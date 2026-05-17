using Microsoft.AspNetCore.Components.Authorization;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Services.Interfaces;
using System.Security.Claims;

namespace ProductManagement.Features.Services.Implementations
{
    public class AppAuthenticationStateProvider : AuthenticationStateProvider
    {
        private User? _currentUser;

        public AppAuthenticationStateProvider()
        {
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = _currentUser == null
                ? new ClaimsIdentity()
                : new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, _currentUser.Username),
                    new Claim(ClaimTypes.Email, _currentUser.Email),
                    new Claim(ClaimTypes.Role, _currentUser.Role?.Name ?? "User"),
                ], "Server authentication");

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }

        public async Task MarkUserAsAuthenticated(User user)
        {
            _currentUser = user;
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
            ], "Server authentication"));

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