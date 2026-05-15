using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithDetailsAsync();
        Task<PagedResult<Product>> GetProductsWithDetailsPagedAsync(int pageNumber, int pageSize);
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<Product?> GetBySkuAsync(string sku);
    }
}
