namespace Shared.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    List<OrderItemEvent> Items,
    DateTime CreatedAt);

public record OrderItemEvent(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record PaymentProcessedEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    bool Success,
    DateTime ProcessedAt);

public record OrderStatusChangedEvent(
    Guid OrderId,
    Guid UserId,
    string OldStatus,
    string NewStatus,
    DateTime ChangedAt);
