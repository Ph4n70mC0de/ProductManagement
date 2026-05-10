using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithDetailsAsync();
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<Product?> GetBySkuAsync(string sku);
    }
}
