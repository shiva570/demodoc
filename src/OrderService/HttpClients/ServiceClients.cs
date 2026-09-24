using Shared.Contracts;
using Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderService.HttpClients;

public interface IUserServiceClient
{
    Task<UserDto?> GetUserAsync(Guid userId);
}

public interface IProductServiceClient
{
    Task<ProductDto?> GetProductAsync(Guid productId);
    Task<bool> ReserveStockAsync(Guid productId, int quantity);
}

public interface IPaymentServiceClient
{
    Task<PaymentDto?> ProcessPaymentAsync(ProcessPaymentRequest request);
}

public interface INotificationServiceClient
{
    Task SendNotificationAsync(SendNotificationRequest request);
}

file record Wrapper<T>(bool Success, T? Data, string? Error);

file static class JsonOpts
{
    internal static readonly JsonSerializerOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };
}

public class UserServiceClient(HttpClient client) : IUserServiceClient
{
    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        var response = await client.GetAsync($"/users/{userId}");
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<Wrapper<UserDto>>(JsonOpts.CaseInsensitive);
        return result?.Data;
    }
}

public class ProductServiceClient(HttpClient client) : IProductServiceClient
{
    public async Task<ProductDto?> GetProductAsync(Guid productId)
    {
        var response = await client.GetAsync($"/products/{productId}");
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<Wrapper<ProductDto>>(JsonOpts.CaseInsensitive);
        return result?.Data;
    }

    public async Task<bool> ReserveStockAsync(Guid productId, int quantity)
    {
        var response = await client.PostAsJsonAsync($"/products/{productId}/reserve", new ReserveStockRequest(productId, quantity));
        return response.IsSuccessStatusCode;
    }
}

public class PaymentServiceClient(HttpClient client) : IPaymentServiceClient
{
    public async Task<PaymentDto?> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var response = await client.PostAsJsonAsync("/payments", request);
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<Wrapper<PaymentDto>>(JsonOpts.CaseInsensitive);
        return result?.Data;
    }
}

public class NotificationServiceClient(HttpClient client) : INotificationServiceClient
{
    public async Task SendNotificationAsync(SendNotificationRequest request)
    {
        await client.PostAsJsonAsync("/notifications", request);
    }
}
