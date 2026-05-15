using System.Text.Json;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<BrandService> _logger;

        public BrandService(IBrandRepository repository, IAuditLogService auditLogService, ILogger<BrandService> logger)
        {
            _repository = repository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve brands");
                return Enumerable.Empty<Brand>();
            }
        }

        public async Task<PagedResult<Brand>> GetBrandsPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _repository.GetPagedAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve brands page {PageNumber}", pageNumber);
                return new PagedResult<Brand>();
            }
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve brand with id {BrandId}", id);
                return null;
            }
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            ValidateBrand(brand);

            try
            {
                brand.CreatedAt = DateTime.UtcNow;
                brand.IsDeleted = false;
                var result = await _repository.AddAsync(brand);
                await _auditLogService.LogActionAsync("Brand", result.Id, "Create", null, JsonSerializer.Serialize(new { result.Name, result.Description }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create brand");
                throw new ServiceException("Failed to create brand", ex);
            }
        }

        public async Task<Brand> UpdateBrandAsync(Brand brand)
        {
            ValidateBrand(brand);

            try
            {
                var oldBrand = await _repository.GetByIdAsync(brand.Id);
                brand.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(brand);
                await _auditLogService.LogActionAsync("Brand", brand.Id, "Update",
                    oldBrand != null ? JsonSerializer.Serialize(new { oldBrand.Name, oldBrand.Description }) : null,
                    JsonSerializer.Serialize(new { result.Name, result.Description }));
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brand with id {BrandId}", brand.Id);
                throw new ServiceException($"Failed to update brand with id {brand.Id}", ex);
            }
        }

        public async Task DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await _repository.GetByIdAsync(id);
                if (brand != null)
                {
                    if (await _repository.HasProductsAsync(id))
                    {
                        throw new InvalidOperationException("Cannot delete a brand that has associated products. Please reassign or delete the products first.");
                    }
                    
                    var oldValues = JsonSerializer.Serialize(new { brand.Name, brand.Description, brand.IsDeleted });
                    brand.IsDeleted = true;
                    brand.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(brand);
                    await _auditLogService.LogActionAsync("Brand", id, "Delete", oldValues, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete brand with id {BrandId}", id);
                throw new ServiceException($"Failed to delete brand with id {id}", ex);
            }
        }

        private static void ValidateBrand(Brand brand)
        {
            ArgumentNullException.ThrowIfNull(brand);
            ValidationHelper.ValidateRequiredString(brand.Name, "Brand name");
        }
    }
}