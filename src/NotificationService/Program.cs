using NotificationService.Services;
using Shared.Contracts;
using Shared.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<INotificationSender, NotificationSender>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Notification Service", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var notifications = app.MapGroup("/notifications").WithTags("Notifications");

notifications.MapGet("/", async (Guid? userId, INotificationSender sender) =>
    Results.Ok(new ApiResponse<object>(true, await sender.GetAllAsync(userId))));

notifications.MapGet("/{id:guid}", async (Guid id, INotificationSender sender) =>
{
    var notification = await sender.GetByIdAsync(id);
    return notification is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "Notification not found"))
        : Results.Ok(new ApiResponse<object>(true, notification));
});

notifications.MapPost("/", async (SendNotificationRequest req, INotificationSender sender) =>
{
    var notification = await sender.SendAsync(req);
    return Results.Created($"/notifications/{notification.Id}", new ApiResponse<object>(true, notification));
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "NotificationService", Timestamp = DateTime.UtcNow }));

app.Run();
