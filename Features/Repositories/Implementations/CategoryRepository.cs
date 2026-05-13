using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Repositories.Interfaces;

namespace ProductManagement.Features.Repositories.Implementations
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>> GetAncestorCategoryIdsAsync(int categoryId)
        {
            var ancestorIds = new List<int>();
            var current = await _context.Categories
                .Where(c => c.Id == categoryId)
                .Select(c => c.ParentCategoryId)
                .FirstOrDefaultAsync();

            while (current.HasValue)
            {
                ancestorIds.Add(current.Value);
                current = await _context.Categories
                    .Where(c => c.Id == current.Value)
                    .Select(c => c.ParentCategoryId)
                    .FirstOrDefaultAsync();
            }

            return ancestorIds;
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId && !p.IsDeleted);
        }

        public async Task<bool> HasSubCategoriesAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId && !c.IsDeleted);
        }
    }
}
