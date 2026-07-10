using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure.External.Notification;

public class NotificationDispatcherService : INotificationService
{
    private readonly IRepository<Core.Entities.Notification> _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationDispatcherService> _logger;

    public NotificationDispatcherService(
        IRepository<Core.Entities.Notification> notificationRepo,
        IUnitOfWork unitOfWork,
        ILogger<NotificationDispatcherService> logger)
    {
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendAsync(Guid userId, string title, string body, NotificationType type,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        await SendToAllAsync([userId], title, body, type, metadata, cancellationToken);
    }

    public async Task SendToAllAsync(IEnumerable<Guid> userIds, string title, string body, NotificationType type,
        Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            var notification = Core.Entities.Notification.Create(
                condominiumId: Guid.Empty,
                userId: userId,
                title: title,
                body: body,
                type: type,
                entityType: metadata?.GetValueOrDefault("entityType"),
                entityId: metadata?.GetValueOrDefault("entityId") is string id ? Guid.Parse(id) : null,
                action: metadata?.GetValueOrDefault("action"),
                channels: "[\"in_app\"]");

            await _notificationRepo.AddAsync(notification, cancellationToken);

            _logger.LogInformation("Notificação criada para usuário {UserId}: {Title}", userId, title);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
