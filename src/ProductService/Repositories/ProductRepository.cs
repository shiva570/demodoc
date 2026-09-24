using ProductService.Models;

namespace ProductService.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(string? category = null);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Guid id, Action<Product> update);
    Task<bool> ReserveStockAsync(Guid id, int quantity);
    Task<bool> DeleteAsync(Guid id);
}

public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products =
    [
        new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), Name = "Laptop Pro 15",    Description = "High-performance laptop",  Price = 1299.99m, StockQuantity = 50,  Category = "Electronics", Sku = "LAP-001" },
        new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), Name = "Wireless Mouse",   Description = "Ergonomic wireless mouse", Price = 29.99m,   StockQuantity = 200, Category = "Electronics", Sku = "MOU-001" },
        new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), Name = "USB-C Hub",        Description = "7-in-1 USB-C hub",        Price = 49.99m,   StockQuantity = 150, Category = "Electronics", Sku = "HUB-001" },
        new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), Name = "Standing Desk",    Description = "Adjustable standing desk", Price = 499.99m, StockQuantity = 20,  Category = "Furniture",   Sku = "DSK-001" },
        new() { Id = Guid.Parse("22222222-0000-0000-0000-000000000005"), Name = "Office Chair",     Description = "Ergonomic office chair",  Price = 349.99m,  StockQuantity = 30,  Category = "Furniture",   Sku = "CHR-001" },
    ];

    public Task<List<Product>> GetAllAsync(string? category = null)
    {
        var query = _products.Where(p => p.IsAvailable);
        if (category is not null)
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(query.ToList());
    }

    public Task<Product?> GetByIdAsync(Guid id) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<Product?> GetBySkuAsync(string sku) =>
        Task.FromResult(_products.FirstOrDefault(p => p.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)));

    public Task<Product> CreateAsync(Product product)
    {
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product?> UpdateAsync(Guid id, Action<Product> update)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult<Product?>(null);
        update(product);
        product.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult<Product?>(product);
    }

    public Task<bool> ReserveStockAsync(Guid id, int quantity)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null || product.StockQuantity < quantity) return Task.FromResult(false);
        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null) return Task.FromResult(false);
        product.IsAvailable = false;
        return Task.FromResult(true);
    }
}
