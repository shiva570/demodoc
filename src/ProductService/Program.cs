using ProductService.Models;
using ProductService.Repositories;
using Shared.Contracts;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Product Service", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var products = app.MapGroup("/products").WithTags("Products");

products.MapGet("/", async (string? category, IProductRepository repo) =>
{
    var items = await repo.GetAllAsync(category);
    return Results.Ok(new ApiResponse<object>(true, items.Select(ToDto)));
});

products.MapGet("/{id:guid}", async (Guid id, IProductRepository repo) =>
{
    var product = await repo.GetByIdAsync(id);
    return product is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Product not found"))
        : Results.Ok(new ApiResponse<object>(true, ToDto(product)));
});

products.MapPost("/", async (CreateProductRequest req, IProductRepository repo) =>
{
    var product = new Product
    {
        Name = req.Name,
        Description = req.Description,
        Price = req.Price,
        StockQuantity = req.StockQuantity,
        Category = req.Category,
        Sku = req.Sku
    };
    var created = await repo.CreateAsync(product);
    return Results.Created($"/products/{created.Id}", new ApiResponse<object>(true, ToDto(created)));
});

products.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest req, IProductRepository repo) =>
{
    var updated = await repo.UpdateAsync(id, p =>
    {
        if (req.Name is not null) p.Name = req.Name;
        if (req.Description is not null) p.Description = req.Description;
        if (req.Price.HasValue) p.Price = req.Price.Value;
        if (req.IsAvailable.HasValue) p.IsAvailable = req.IsAvailable.Value;
    });
    return updated is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Product not found"))
        : Results.Ok(new ApiResponse<object>(true, ToDto(updated)));
});

products.MapPost("/{id:guid}/reserve", async (Guid id, ReserveStockRequest req, IProductRepository repo) =>
{
    var reserved = await repo.ReserveStockAsync(id, req.Quantity);
    return reserved
        ? Results.Ok(new ReserveStockResponse(true, null))
        : Results.BadRequest(new ReserveStockResponse(false, "Insufficient stock"));
});

products.MapDelete("/{id:guid}", async (Guid id, IProductRepository repo) =>
{
    var deleted = await repo.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "ProductService", Timestamp = DateTime.UtcNow }));

app.Run();

static ProductDto ToDto(Product p) => new(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.Category);
