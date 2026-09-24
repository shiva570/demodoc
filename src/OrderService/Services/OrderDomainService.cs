using OrderService.HttpClients;
using OrderService.Models;
using OrderService.Repositories;
using Shared.Contracts;
using Shared.Models;

namespace OrderService.Services;

public interface IOrderDomainService
{
    Task<List<OrderDto>> GetOrdersAsync(Guid? userId = null);
    Task<OrderDto?> GetOrderAsync(Guid id);
    Task<(OrderDto? Order, string? Error)> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderDto?> UpdateOrderStatusAsync(Guid id, string status);
}

public class OrderDomainService(
    IOrderRepository repository,
    IUserServiceClient userClient,
    IProductServiceClient productClient,
    IPaymentServiceClient paymentClient,
    INotificationServiceClient notificationClient,
    ILogger<OrderDomainService> logger) : IOrderDomainService
{
    public async Task<List<OrderDto>> GetOrdersAsync(Guid? userId = null)
    {
        var orders = await repository.GetAllAsync(userId);
        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderAsync(Guid id)
    {
        var order = await repository.GetByIdAsync(id);
        return order is null ? null : ToDto(order);
    }

    public async Task<(OrderDto? Order, string? Error)> CreateOrderAsync(CreateOrderRequest request)
    {
        // 1. Validate user exists
        var user = await userClient.GetUserAsync(request.UserId);
        if (user is null)
            return (null, $"User {request.UserId} not found");

        // 2. Validate products and calculate totals
        var lines = new List<OrderLine>();
        foreach (var lineReq in request.Lines)
        {
            var product = await productClient.GetProductAsync(lineReq.ProductId);
            if (product is null)
                return (null, $"Product {lineReq.ProductId} not found");

            lines.Add(new OrderLine
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = lineReq.Quantity,
                UnitPrice = product.Price
            });
        }

        decimal totalAmount = lines.Sum(l => l.LineTotal);

        // 3. Reserve stock for all products
        foreach (var line in lines)
        {
            var reserved = await productClient.ReserveStockAsync(line.ProductId, line.Quantity);
            if (!reserved)
                return (null, $"Insufficient stock for product {line.ProductName}");
        }

        // 4. Create order record
        var order = new Order
        {
            UserId = request.UserId,
            UserName = user.Name,
            Lines = lines,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending
        };
        order = await repository.CreateAsync(order);

        // 5. Process payment
        var payment = await paymentClient.ProcessPaymentAsync(new ProcessPaymentRequest(
            order.Id, request.UserId, totalAmount, "CreditCard"));

        if (payment is null || payment.Status != "Completed")
        {
            await repository.UpdateStatusAsync(order.Id, OrderStatus.Cancelled);
            logger.LogWarning("Payment failed for order {OrderId}", order.Id);
            return (null, "Payment processing failed");
        }

        // 6. Confirm order
        order = (await repository.UpdateStatusAsync(order.Id, OrderStatus.Confirmed, payment.Id.ToString()))!;

        // 7. Send notification asynchronously (fire and forget in demo)
        _ = notificationClient.SendNotificationAsync(new SendNotificationRequest(
            request.UserId,
            "Email",
            $"Order #{order.Id} Confirmed",
            $"Dear {user.Name}, your order for {lines.Count} item(s) totalling ${totalAmount:F2} has been confirmed."));

        logger.LogInformation("Order {OrderId} created successfully for user {UserId}", order.Id, request.UserId);
        return (ToDto(order), null);
    }

    public async Task<OrderDto?> UpdateOrderStatusAsync(Guid id, string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            return null;

        var order = await repository.UpdateStatusAsync(id, orderStatus);
        return order is null ? null : ToDto(order);
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id,
        o.UserId,
        o.UserName,
        o.Lines.Select(l => new OrderLineDto(l.ProductId, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal)).ToList(),
        o.TotalAmount,
        o.Status.ToString(),
        o.CreatedAt,
        o.UpdatedAt);
}
