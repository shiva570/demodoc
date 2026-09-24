using OrderService.Models;
using Shared.Models;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetAllAsync(Guid? userId = null);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(Order order);
    Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, string? paymentId = null);
}

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];

    public Task<List<Order>> GetAllAsync(Guid? userId = null)
    {
        var query = _orders.AsEnumerable();
        if (userId.HasValue) query = query.Where(o => o.UserId == userId.Value);
        return Task.FromResult(query.OrderByDescending(o => o.CreatedAt).ToList());
    }

    public Task<Order?> GetByIdAsync(Guid id) =>
        Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

    public Task<Order> CreateAsync(Order order)
    {
        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, string? paymentId = null)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null) return Task.FromResult<Order?>(null);
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        if (paymentId is not null) order.PaymentId = paymentId;
        return Task.FromResult<Order?>(order);
    }
}
