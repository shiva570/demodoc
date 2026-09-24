using PaymentService.Models;
using PaymentService.Repositories;
using Shared.Contracts;
using Shared.Models;

namespace PaymentService.Services;

public interface IPaymentProcessor
{
    Task<PaymentDto> ProcessAsync(ProcessPaymentRequest request);
    Task<PaymentDto?> GetPaymentAsync(Guid id);
    Task<PaymentDto?> GetPaymentByOrderAsync(Guid orderId);
    Task<List<PaymentDto>> GetAllPaymentsAsync();
    Task<PaymentDto?> RefundAsync(Guid paymentId);
}

public class PaymentProcessor(
    IPaymentRepository repository,
    ILogger<PaymentProcessor> logger) : IPaymentProcessor
{
    public async Task<PaymentDto> ProcessAsync(ProcessPaymentRequest request)
    {
        var payment = new Payment
        {
            OrderId = request.OrderId,
            UserId = request.UserId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            Status = PaymentStatus.Pending
        };

        payment = await repository.CreateAsync(payment);

        // Simulate payment gateway call (90% success rate for demo)
        await Task.Delay(50); // Simulate network latency
        var isSuccess = new Random().Next(10) < 9;

        await repository.UpdateAsync(payment.Id, p =>
        {
            p.Status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed;
            p.ProcessedAt = DateTime.UtcNow;
            p.TransactionReference = isSuccess ? $"TXN-{Guid.NewGuid():N}".ToUpper()[..20] : null;
            p.FailureReason = isSuccess ? null : "Payment gateway declined";
        });

        var updated = (await repository.GetByIdAsync(payment.Id))!;
        logger.LogInformation("Payment {PaymentId} for order {OrderId}: {Status}",
            payment.Id, request.OrderId, updated.Status);

        return ToDto(updated);
    }

    public async Task<PaymentDto?> GetPaymentAsync(Guid id)
    {
        var payment = await repository.GetByIdAsync(id);
        return payment is null ? null : ToDto(payment);
    }

    public async Task<PaymentDto?> GetPaymentByOrderAsync(Guid orderId)
    {
        var payment = await repository.GetByOrderIdAsync(orderId);
        return payment is null ? null : ToDto(payment);
    }

    public async Task<List<PaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await repository.GetAllAsync();
        return payments.Select(ToDto).ToList();
    }

    public async Task<PaymentDto?> RefundAsync(Guid paymentId)
    {
        var updated = await repository.UpdateAsync(paymentId, p =>
        {
            p.Status = PaymentStatus.Refunded;
            p.ProcessedAt = DateTime.UtcNow;
        });
        return updated is null ? null : ToDto(updated);
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id, p.OrderId, p.UserId, p.Amount,
        p.Status.ToString(), p.TransactionReference, p.CreatedAt);
}
