using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve categories");
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve category with id {CategoryId}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            try
            {
                return await _repository.GetRootCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve root categories");
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
        {
            try
            {
                return await _repository.GetSubCategoriesAsync(parentId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve sub categories for parent id {ParentId}", parentId);
                return Enumerable.Empty<Category>();
            }
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            try
            {
                category.CreatedAt = DateTime.UtcNow;
                category.IsDeleted = false;
                return await _repository.AddAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create category");
                throw new ServiceException("Failed to create category", ex);
            }
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            try
            {
                category.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update category with id {CategoryId}", category.Id);
                throw new ServiceException($"Failed to update category with id {category.Id}", ex);
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _repository.GetByIdAsync(id);
                if (category != null)
                {
                    category.IsDeleted = true;
                    category.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(category);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete category with id {CategoryId}", id);
                throw new ServiceException($"Failed to delete category with id {id}", ex);
            }
        }
    }
}