using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Repositories.Interfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<bool> HasProductsAsync(int supplierId);
    }
}
