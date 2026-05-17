using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Repositories.Interfaces;

namespace ProductManagement.Features.Repositories.Implementations
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> HasProductsAsync(int supplierId)
        {
            return await _context.Products.AnyAsync(p => p.SupplierId == supplierId && !p.IsDeleted);
        }

        public override async Task<PagedResult<Supplier>> GetPagedAsync(int pageNumber, int pageSize, string? searchString = null, bool? isActive = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Suppliers
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(s => s.Name.Contains(searchString) || s.ContactPerson.Contains(searchString) || s.Email.Contains(searchString));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<Supplier>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
