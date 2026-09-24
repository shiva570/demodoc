using Shared.Models;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<IUserService, UserDomainService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "User Service", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

var users = app.MapGroup("/users").WithTags("Users");

users.MapGet("/", async (IUserService svc) =>
    Results.Ok(new ApiResponse<object>(true, await svc.GetAllUsersAsync())));

users.MapGet("/{id:guid}", async (Guid id, IUserService svc) =>
{
    var user = await svc.GetUserByIdAsync(id);
    return user is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "User not found"))
        : Results.Ok(new ApiResponse<object>(true, user));
});

users.MapPost("/", async (CreateUserRequest req, IUserService svc) =>
{
    var user = await svc.CreateUserAsync(req);
    return Results.Created($"/users/{user.Id}", new ApiResponse<object>(true, user));
});

users.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest req, IUserService svc) =>
{
    var user = await svc.UpdateUserAsync(id, req);
    return user is null
        ? Results.NotFound(new ApiResponse<object>(false, null, "User not found"))
        : Results.Ok(new ApiResponse<object>(true, user));
});

users.MapDelete("/{id:guid}", async (Guid id, IUserService svc) =>
{
    var deleted = await svc.DeleteUserAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

users.MapPost("/authenticate", async (AuthenticateRequest req, IUserService svc) =>
{
    var result = await svc.AuthenticateAsync(req);
    return result.Success
        ? Results.Ok(new ApiResponse<object>(true, result))
        : Results.Unauthorized();
});

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = "UserService", Timestamp = DateTime.UtcNow }));

app.Run();
