using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name);
    }
}