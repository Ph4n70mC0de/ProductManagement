using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetRootCategoriesAsync();
        Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<int>> GetAncestorCategoryIdsAsync(int categoryId);
        Task<bool> HasProductsAsync(int categoryId);
        Task<bool> HasSubCategoriesAsync(int categoryId);
    }
}
