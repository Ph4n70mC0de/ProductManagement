using System.ComponentModel.DataAnnotations;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
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
            ValidateCategory(category);
            await ValidateCategoryHierarchyAsync(category);

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
            ValidateCategory(category);
            await ValidateCategoryHierarchyAsync(category);

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

        private static void ValidateCategory(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);
            ValidationHelper.ValidateRequiredString(category.Name, "Category name");

            if (category.ParentCategoryId.HasValue && category.ParentCategoryId.Value == category.Id)
                throw new ValidationException("A category cannot be its own parent");
        }

        private async Task ValidateCategoryHierarchyAsync(Category category)
        {
            if (!category.ParentCategoryId.HasValue)
                return;

            var ancestorIds = await _repository.GetAncestorCategoryIdsAsync(category.ParentCategoryId.Value);
            if (ancestorIds.Contains(category.Id))
                throw new ValidationException("A category cannot be an ancestor of itself (circular reference detected)");
        }
    }
}