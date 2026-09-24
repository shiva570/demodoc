using NotificationService.Models;
using Shared.Contracts;
using Shared.Models;

namespace NotificationService.Services;

public interface INotificationSender
{
    Task<NotificationDto> SendAsync(SendNotificationRequest request);
    Task<List<NotificationDto>> GetAllAsync(Guid? userId = null);
    Task<NotificationDto?> GetByIdAsync(Guid id);
}

public class NotificationSender(ILogger<NotificationSender> logger) : INotificationSender
{
    private readonly List<Notification> _notifications = [];

    public async Task<NotificationDto> SendAsync(SendNotificationRequest request)
    {
        if (!Enum.TryParse<NotificationType>(request.Type, true, out var type))
            type = NotificationType.Email;

        var notification = new Notification
        {
            UserId = request.UserId,
            Type = type,
            Subject = request.Subject,
            Body = request.Body
        };

        _notifications.Add(notification);

        // Simulate delivery (email provider, SMS gateway, etc.)
        await Task.Delay(20);
        notification.Sent = true;
        notification.SentAt = DateTime.UtcNow;

        logger.LogInformation(
            "[{Type}] Notification sent to user {UserId}: {Subject}",
            type, request.UserId, request.Subject);

        return ToDto(notification);
    }

    public Task<List<NotificationDto>> GetAllAsync(Guid? userId = null)
    {
        var query = _notifications.AsEnumerable();
        if (userId.HasValue) query = query.Where(n => n.UserId == userId.Value);
        return Task.FromResult(query.OrderByDescending(n => n.CreatedAt).Select(ToDto).ToList());
    }

    public Task<NotificationDto?> GetByIdAsync(Guid id)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == id);
        return Task.FromResult(notification is null ? null : ToDto(notification));
    }

    private static NotificationDto ToDto(Notification n) => new(
        n.Id, n.UserId, n.Type.ToString(), n.Subject, n.Body, n.Sent, n.CreatedAt);
}
