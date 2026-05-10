using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces;

public interface IDataExportService
{
    byte[] ExportProductsToCsv(IEnumerable<Product> products);
    byte[] ExportBrandsToCsv(IEnumerable<Brand> brands);
    byte[] ExportCategoriesToCsv(IEnumerable<Category> categories);
    byte[] ExportSuppliersToCsv(IEnumerable<Supplier> suppliers);
}