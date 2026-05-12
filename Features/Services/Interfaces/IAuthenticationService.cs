using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> LoginAsync(string username, string password);
        Task LogoutAsync();
    }

    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public User? User { get; set; }
    }
}