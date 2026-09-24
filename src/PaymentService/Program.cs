using PaymentService.Repositories;
using PaymentService.Services;
using Shared.Contracts;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddScoped<IPaymentProcessor, PaymentProcessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Payment Service", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var payments = app.MapGroup("/payments").WithTags("Payments");

payments.MapGet("/", async (IPaymentProcessor processor) =>
    Results.Ok(new ApiResponse<object>(true, await processor.GetAllPaymentsAsync())));

payments.MapGet("/{id:guid}", async (Guid id, IPaymentProcessor processor) =>
{
    var payment = await processor.GetPaymentAsync(id);
    return payment is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Payment not found"))
        : Results.Ok(new ApiResponse<object>(true, payment));
});

payments.MapGet("/order/{orderId:guid}", async (Guid orderId, IPaymentProcessor processor) =>
{
    var payment = await processor.GetPaymentByOrderAsync(orderId);
    return payment is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Payment not found for order"))
        : Results.Ok(new ApiResponse<object>(true, payment));
});

payments.MapPost("/", async (ProcessPaymentRequest req, IPaymentProcessor processor) =>
{
    var payment = await processor.ProcessAsync(req);
    return payment.Status == "Completed"
        ? Results.Ok(new ApiResponse<object>(true, payment))
        : Results.BadRequest(new ApiResponse<object>(false, payment, "Payment failed"));
});

payments.MapPost("/{id:guid}/refund", async (Guid id, IPaymentProcessor processor) =>
{
    var payment = await processor.RefundAsync(id);
    return payment is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Payment not found"))
        : Results.Ok(new ApiResponse<object>(true, payment));
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "PaymentService", Timestamp = DateTime.UtcNow }));

app.Run();
