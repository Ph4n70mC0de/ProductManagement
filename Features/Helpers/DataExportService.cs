using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Services.Interfaces;

public interface IDataExportService
{
    byte[] ExportProductsToCsv(IEnumerable<Product> products);
    byte[] ExportBrandsToCsv(IEnumerable<Brand> brands);
    byte[] ExportCategoriesToCsv(IEnumerable<Category> categories);
    byte[] ExportSuppliersToCsv(IEnumerable<Supplier> suppliers);
}

namespace ProductManagement.Features.Services.Implementations;

public class DataExportService : IDataExportService
{
    public byte[] ExportProductsToCsv(IEnumerable<Product> products)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        
        csv.WriteRecords(products.Select(p => new
        {
            p.Id,
            p.Name,
            p.SKU,
            p.Description,
            p.Price,
            p.Cost,
            p.Quantity,
            Brand = p.Brand?.Name,
            Category = p.Category?.Name,
            Supplier = p.Supplier?.Name,
            p.IsActive,
            p.CreatedAt,
            p.UpdatedAt
        }));
        
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public byte[] ExportBrandsToCsv(IEnumerable<Brand> brands)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(brands);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public byte[] ExportCategoriesToCsv(IEnumerable<Category> categories)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(categories);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }

    public byte[] ExportSuppliersToCsv(IEnumerable<Supplier> suppliers)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(suppliers);
        return System.Text.Encoding.UTF8.GetBytes(writer.ToString());
    }
}