namespace Shared.Models;

public record ApiResponse<T>(bool Success, T? Data, string? Error = null);

public enum OrderStatus { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled }
public enum PaymentStatus { Pending, Completed, Failed, Refunded }
public enum NotificationType { Email, SMS, Push }
