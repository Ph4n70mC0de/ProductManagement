using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ProductManagement.Features.Data;
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
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repository, IAuditLogService auditLogService, ILogger<ProductService> logger)
        {
            _repository = repository;
            _auditLogService = auditLogService;
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

        public async Task<PagedResult<Product>> GetProductsPagedAsync(int pageNumber, int pageSize, string? searchString = null)
        {
            try
            {
                return await _repository.GetProductsWithDetailsPagedAsync(pageNumber, pageSize, searchString);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve products page {PageNumber}", pageNumber);
                return new PagedResult<Product>();
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
                var result = await _repository.AddAsync(product);
                await _auditLogService.LogActionAsync("Product", result.Id, "Create", null, JsonSerializer.Serialize(new { result.Id, result.Name, result.SKU, result.Price, result.Quantity }));
                return result;
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
                var oldProduct = await _repository.GetByIdAsync(product.Id);
                product.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(product);
                await _auditLogService.LogActionAsync("Product", product.Id, "Update", 
                    oldProduct != null ? JsonSerializer.Serialize(new { oldProduct.Name, oldProduct.SKU, oldProduct.Price, oldProduct.Quantity }) : null,
                    JsonSerializer.Serialize(new { result.Name, result.SKU, result.Price, result.Quantity }));
                return result;
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
                    var oldValues = JsonSerializer.Serialize(new { product.Name, product.SKU, product.IsDeleted });
                    product.IsDeleted = true;
                    product.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(product);
                    await _auditLogService.LogActionAsync("Product", id, "Delete", oldValues, null);
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

        public async Task<string> UploadProductImageAsync(int productId, Stream fileStream, string fileName)
        {
            try
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                    throw new ValidationException("Invalid file type. Allowed types: jpg, jpeg, png, gif");

                var product = await _repository.GetByIdAsync(productId);
                if (product == null)
                    throw new ValidationException("Product not found");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, $"{productId}_{Guid.NewGuid()}{extension}");
                using var fileStreamOutput = File.Create(filePath);
                await fileStream.CopyToAsync(fileStreamOutput);

                var relativePath = $"/images/products/{Path.GetFileName(filePath)}";
                product.ImageUrl = relativePath;
                await _repository.UpdateAsync(product);

                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for product {ProductId}", productId);
                throw new ServiceException("Failed to upload product image", ex);
            }
        }
    }
}