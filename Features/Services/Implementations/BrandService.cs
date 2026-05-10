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
        private readonly ILogger<BrandService> _logger;

        public BrandService(IBrandRepository repository, ILogger<BrandService> logger)
        {
            _repository = repository;
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
                return await _repository.AddAsync(brand);
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
                brand.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(brand);
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
                    brand.IsDeleted = true;
                    brand.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(brand);
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