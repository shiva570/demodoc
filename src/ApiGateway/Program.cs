var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.UseCors();
app.MapReverseProxy();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "ApiGateway",
    Timestamp = DateTime.UtcNow,
    Routes = new[]
    {
        "/api/users/** → UserService:5001",
        "/api/products/** → ProductService:5002",
        "/api/orders/** → OrderService:5003",
        "/api/payments/** → PaymentService:5004",
        "/api/notifications/** → NotificationService:5005"
    }
}));

app.Run();
