using Microsoft.EntityFrameworkCore;
using ProductManagement.Features.Data.Model;

namespace ProductManagement.Features.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>());

        context.Database.EnsureCreated();

        if (context.Brands.Any(b => !b.IsDeleted)) return;

        var roles = new[]
        {
            new Role { Name = "Admin", Description = "Full access to all features", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Name = "Manager", Description = "Manage products and inventory", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Role { Name = "Staff", Description = "View and basic operations", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Roles.AddRange(roles);
        context.SaveChanges();

        var brands = new[]
        {
            new Brand { Name = "Apple", Description = "Innovative technology products", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Brand { Name = "Samsung", Description = "Electronics and mobile devices", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Brand { Name = "Nike", Description = "Athletic footwear and apparel", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Brand { Name = "Sony", Description = "Entertainment and electronics", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Brands.AddRange(brands);

        var categories = new[]
        {
            new Category { Name = "Electronics", Description = "Electronic devices and accessories", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Clothing", Description = "Apparel and fashion items", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Computers", Description = "Desktops, laptops and accessories", IsActive = true, CreatedAt = DateTime.UtcNow, ParentCategoryId = 1 },
            new Category { Name = "Smartphones", Description = "Mobile phones and accessories", IsActive = true, CreatedAt = DateTime.UtcNow, ParentCategoryId = 1 },
            new Category { Name = "Shoes", Description = "Athletic and casual footwear", IsActive = true, CreatedAt = DateTime.UtcNow, ParentCategoryId = 2 }
        };
        context.Categories.AddRange(categories);

        var suppliers = new[]
        {
            new Supplier { Name = "Tech Distributors Inc", ContactPerson = "John Smith", Email = "john@techdist.com", Phone = "555-0101", Address = "123 Tech St", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Supplier { Name = "Global Fashion LLC", ContactPerson = "Jane Doe", Email = "jane@gfashion.com", Phone = "555-0102", Address = "456 Fashion Ave", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Supplier { Name = "Electro Supply Co", ContactPerson = "Bob Wilson", Email = "bob@electrosupply.com", Phone = "555-0103", Address = "789 Electric Blvd", IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Suppliers.AddRange(suppliers);

        context.SaveChanges();

        var products = new[]
        {
            new Product { Name = "iPhone 15 Pro", Description = "Latest iPhone model", SKU = "APL-PHN-001", Price = 999.99m, Cost = 750.00m, Quantity = 50, BrandId = 1, CategoryId = 4, SupplierId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "MacBook Air M2", Description = "13-inch laptop", SKU = "APL-COM-001", Price = 1299.99m, Cost = 900.00m, Quantity = 30, BrandId = 1, CategoryId = 3, SupplierId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Galaxy S24", Description = "Android smartphone", SKU = "SAM-PHN-001", Price = 899.99m, Cost = 650.00m, Quantity = 45, BrandId = 2, CategoryId = 4, SupplierId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Air Max 270", Description = "Running shoes", SKU = "NIK-SHO-001", Price = 150.00m, Cost = 80.00m, Quantity = 100, BrandId = 3, CategoryId = 5, SupplierId = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Name = "WH-1000XM5", Description = "Wireless headphones", SKU = "SON-AUD-001", Price = 399.99m, Cost = 250.00m, Quantity = 60, BrandId = 4, CategoryId = 1, SupplierId = 3, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        context.Products.AddRange(products);

        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@productmanagement.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        context.SaveChanges();
    }
}