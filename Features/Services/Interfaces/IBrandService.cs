using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<PagedResult<Brand>> GetBrandsPagedAsync(int pageNumber, int pageSize, string? searchString = null, CancellationToken cancellationToken = default);
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<Brand> UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(int id);
    }
}