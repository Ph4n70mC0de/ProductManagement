using System.ComponentModel.DataAnnotations;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repository, ILogger<ProductService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            try
            {
                return await _repository.GetProductsWithDetailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve products");
                return Enumerable.Empty<Product>();
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _repository.GetProductWithDetailsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve product with id {ProductId}", id);
                return null;
            }
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await ValidateProductAsync(product);

            try
            {
                product.CreatedAt = DateTime.UtcNow;
                product.IsDeleted = false;
                return await _repository.AddAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product");
                throw new ServiceException("Failed to create product", ex);
            }
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            await ValidateProductAsync(product);

            try
            {
                product.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product with id {ProductId}", product.Id);
                throw new ServiceException($"Failed to update product with id {product.Id}", ex);
            }
        }

        public async Task DeleteProductAsync(int id)
        {
            try
            {
                var product = await _repository.GetByIdAsync(id);
                if (product != null)
                {
                    product.IsDeleted = true;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(product);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product with id {ProductId}", id);
                throw new ServiceException($"Failed to delete product with id {id}", ex);
            }
        }

        private static void ValidateProduct(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);

            ValidationHelper.ValidateRequiredString(product.Name, "Product name");
            ValidationHelper.ValidateRequiredString(product.SKU, "SKU");

            if (product.Price < 0)
                throw new ValidationException("Price cannot be negative");

            if (product.Cost < 0)
                throw new ValidationException("Cost cannot be negative");

            if (product.Quantity < 0)
                throw new ValidationException("Quantity cannot be negative");

            if (product.BrandId <= 0)
                throw new ValidationException("Brand is required");

            if (product.CategoryId <= 0)
                throw new ValidationException("Category is required");

            if (product.SupplierId <= 0)
                throw new ValidationException("Supplier is required");
        }

        private async Task ValidateProductAsync(Product product)
        {
            ValidateProduct(product);

            var existing = await _repository.GetBySkuAsync(product.SKU);
            if (existing != null && existing.Id != product.Id)
                throw new ValidationException($"A product with SKU '{product.SKU}' already exists");
        }
    }
}