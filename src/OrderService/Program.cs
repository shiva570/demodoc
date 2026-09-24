using OrderService.HttpClients;
using OrderService.Repositories;
using OrderService.Services;
using Shared.Contracts;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddScoped<IOrderDomainService, OrderDomainService>();

builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>(c =>
    c.BaseAddress = new Uri(cfg["Services:UserService"] ?? "http://localhost:5001"));

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(c =>
    c.BaseAddress = new Uri(cfg["Services:ProductService"] ?? "http://localhost:5002"));

builder.Services.AddHttpClient<IPaymentServiceClient, PaymentServiceClient>(c =>
    c.BaseAddress = new Uri(cfg["Services:PaymentService"] ?? "http://localhost:5004"));

builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(c =>
    c.BaseAddress = new Uri(cfg["Services:NotificationService"] ?? "http://localhost:5005"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Order Service", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var orders = app.MapGroup("/orders").WithTags("Orders");

orders.MapGet("/", async (Guid? userId, IOrderDomainService svc) =>
    Results.Ok(new ApiResponse<object>(true, await svc.GetOrdersAsync(userId))));

orders.MapGet("/{id:guid}", async (Guid id, IOrderDomainService svc) =>
{
    var order = await svc.GetOrderAsync(id);
    return order is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Order not found"))
        : Results.Ok(new ApiResponse<object>(true, order));
});

orders.MapPost("/", async (CreateOrderRequest req, IOrderDomainService svc) =>
{
    var (order, error) = await svc.CreateOrderAsync(req);
    return order is null
        ? Results.BadRequest(new ApiResponse<object>(false, null, error))
        : Results.Created($"/orders/{order.Id}", new ApiResponse<object>(true, order));
});

orders.MapPut("/{id:guid}/status", async (Guid id, UpdateStatusRequest req, IOrderDomainService svc) =>
{
    var order = await svc.UpdateOrderStatusAsync(id, req.Status);
    return order is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Order not found"))
        : Results.Ok(new ApiResponse<object>(true, order));
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "OrderService", Timestamp = DateTime.UtcNow }));

app.Run();

record UpdateStatusRequest(string Status);
