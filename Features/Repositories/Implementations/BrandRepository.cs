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

        public override async Task<PagedResult<Brand>> GetPagedAsync(int pageNumber, int pageSize, string? searchString = null, bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Brands
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(b => b.Name.Contains(searchString) || (b.Description != null && b.Description.Contains(searchString)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Brand>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
