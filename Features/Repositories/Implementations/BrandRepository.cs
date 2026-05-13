using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Repositories.Interfaces;

namespace ProductManagement.Features.Repositories.Implementations
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        public BrandRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> HasProductsAsync(int brandId)
        {
            return await _context.Products.AnyAsync(p => p.BrandId == brandId && !p.IsDeleted);
        }
    }
}
