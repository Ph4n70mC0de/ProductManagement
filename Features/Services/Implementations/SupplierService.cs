using ProductManagement.Features.Data.Model;
using ProductManagement.Features.Helpers;
using ProductManagement.Features.Helpers.Exceptions;
using ProductManagement.Features.Repositories.Interfaces;
using ProductManagement.Features.Services.Interfaces;

namespace ProductManagement.Features.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepository repository, ILogger<SupplierService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve suppliers");
                return Enumerable.Empty<Supplier>();
            }
        }

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve supplier with id {SupplierId}", id);
                return null;
            }
        }

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            ValidateSupplier(supplier);

            try
            {
                supplier.CreatedAt = DateTime.UtcNow;
                supplier.IsDeleted = false;
                return await _repository.AddAsync(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create supplier");
                throw new ServiceException("Failed to create supplier", ex);
            }
        }

        public async Task<Supplier> UpdateSupplierAsync(Supplier supplier)
        {
            ValidateSupplier(supplier);

            try
            {
                supplier.UpdatedAt = DateTime.UtcNow;
                return await _repository.UpdateAsync(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update supplier with id {SupplierId}", supplier.Id);
                throw new ServiceException($"Failed to update supplier with id {supplier.Id}", ex);
            }
        }

        public async Task DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _repository.GetByIdAsync(id);
                if (supplier != null)
                {
                    supplier.IsDeleted = true;
                    supplier.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(supplier);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete supplier with id {SupplierId}", id);
                throw new ServiceException($"Failed to delete supplier with id {id}", ex);
            }
        }

        private static void ValidateSupplier(Supplier supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);

            ValidationHelper.ValidateRequiredString(supplier.Name, "Supplier name");
            ValidationHelper.ValidateRequiredString(supplier.ContactPerson, "Contact person");
            ValidationHelper.ValidateEmail(supplier.Email, "Email");
            ValidationHelper.ValidateRequiredString(supplier.Phone, "Phone");
            ValidationHelper.ValidateRequiredString(supplier.Address, "Address");
        }
    }
}