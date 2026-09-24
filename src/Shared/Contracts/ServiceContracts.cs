namespace Shared.Contracts;

// Contracts used across service boundaries via HTTP

public record UserDto(Guid Id, string Name, string Email, DateTime CreatedAt);

public record ProductDto(Guid Id, string Name, string Description, decimal Price, int StockQuantity, string Category);

public record OrderDto(
    Guid Id,
    Guid UserId,
    string UserName,
    List<OrderLineDto> Lines,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record OrderLineDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    Guid UserId,
    decimal Amount,
    string Status,
    string? TransactionReference,
    DateTime CreatedAt);

public record NotificationDto(
    Guid Id,
    Guid UserId,
    string Type,
    string Subject,
    string Body,
    bool Sent,
    DateTime CreatedAt);

// Request contracts
public record CreateOrderRequest(Guid UserId, List<CreateOrderLineRequest> Lines);
public record CreateOrderLineRequest(Guid ProductId, int Quantity);
public record ProcessPaymentRequest(Guid OrderId, Guid UserId, decimal Amount, string PaymentMethod);
public record SendNotificationRequest(Guid UserId, string Type, string Subject, string Body);
public record UpdateStockRequest(int QuantityDelta);
public record ReserveStockRequest(Guid ProductId, int Quantity);
public record ReserveStockResponse(bool Success, string? Reason);
