using System.Text.Json;
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
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepository repository, IAuditLogService auditLogService, ILogger<SupplierService> logger)
        {
            _repository = repository;
            _auditLogService = auditLogService;
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
                var result = await _repository.AddAsync(supplier);
                await _auditLogService.LogActionAsync("Supplier", result.Id, "Create", null, JsonSerializer.Serialize(new { result.Name, result.ContactPerson, result.Email }));
                return result;
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
                var oldSupplier = await _repository.GetByIdAsync(supplier.Id);
                supplier.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(supplier);
                await _auditLogService.LogActionAsync("Supplier", supplier.Id, "Update",
                    oldSupplier != null ? JsonSerializer.Serialize(new { oldSupplier.Name, oldSupplier.ContactPerson, oldSupplier.Email }) : null,
                    JsonSerializer.Serialize(new { result.Name, result.ContactPerson, result.Email }));
                return result;
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
                    if (await _repository.HasProductsAsync(id))
                    {
                        throw new InvalidOperationException("Cannot delete a supplier that has associated products. Please reassign or delete the products first.");
                    }
                    
                    var oldValues = JsonSerializer.Serialize(new { supplier.Name, supplier.ContactPerson, supplier.IsDeleted });
                    supplier.IsDeleted = true;
                    supplier.UpdatedAt = DateTime.UtcNow;
                    await _repository.UpdateAsync(supplier);
                    await _auditLogService.LogActionAsync("Supplier", id, "Delete", oldValues, null);
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