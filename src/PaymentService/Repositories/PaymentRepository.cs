using PaymentService.Models;
using Shared.Models;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<List<Payment>> GetAllAsync();
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment?> UpdateAsync(Guid id, Action<Payment> update);
}

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = [];

    public Task<List<Payment>> GetAllAsync() =>
        Task.FromResult(_payments.OrderByDescending(p => p.CreatedAt).ToList());

    public Task<Payment?> GetByIdAsync(Guid id) =>
        Task.FromResult(_payments.FirstOrDefault(p => p.Id == id));

    public Task<Payment?> GetByOrderIdAsync(Guid orderId) =>
        Task.FromResult(_payments.FirstOrDefault(p => p.OrderId == orderId));

    public Task<Payment> CreateAsync(Payment payment)
    {
        _payments.Add(payment);
        return Task.FromResult(payment);
    }

    public Task<Payment?> UpdateAsync(Guid id, Action<Payment> update)
    {
        var payment = _payments.FirstOrDefault(p => p.Id == id);
        if (payment is null) return Task.FromResult<Payment?>(null);
        update(payment);
        return Task.FromResult<Payment?>(payment);
    }
}
