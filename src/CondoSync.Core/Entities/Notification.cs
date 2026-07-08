using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Notification : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UserId { get; private set; }

    public string Title { get; private set; }
    public string? Body { get; private set; }
    public CondoSync.Core.Enums.NotificationType Type { get; private set; }

    // Relacionamento
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? Action { get; private set; }

    // Entrega
    public string? Channels { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    // Status
    public bool IsRead { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Notification() { }

    public static Notification Create(
        Guid condominiumId,
        Guid userId,
        string title,
        string? body,
        CondoSync.Core.Enums.NotificationType type,
        string? entityType = null,
        Guid? entityId = null,
        string? action = null,
        string channels = "[\"in_app\"]")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título da notificação não pode ser vazio");

        return new Notification
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Channels = channels,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Send()
    {
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
    }
}