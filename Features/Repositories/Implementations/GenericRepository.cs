using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Data;
using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Repositories.Interfaces;

namespace ProductManagement.Features.Repositories.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = _dbSet.AsQueryable();
            
            // Filter out soft-deleted entities if the entity has IsDeleted property
            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
            }
            
            return await query.ToListAsync();
        }

        public virtual async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _dbSet.AsQueryable();
            
            // Filter out soft-deleted entities if the entity has IsDeleted property
            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
            }
            
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            var isDeletedProperty = typeof(T).GetProperty("IsDeleted");
            if (isDeletedProperty != null)
            {
                isDeletedProperty.SetValue(entity, true);
                var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
                if (updatedAtProperty != null)
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                _dbSet.Update(entity);
            }
            else
            {
                _dbSet.Remove(entity);
            }
            await _context.SaveChangesAsync();
        }
    }
}
