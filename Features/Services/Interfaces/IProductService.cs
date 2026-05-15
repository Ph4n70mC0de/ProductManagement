using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<PagedResult<Product>> GetProductsPagedAsync(int pageNumber, int pageSize);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<string> UploadProductImageAsync(int productId, Stream fileStream, string fileName);
    }
}
