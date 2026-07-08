namespace CondoSync.Core.Entities;

public class ActivityLog : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid? UserId { get; private set; }

    public string Action { get; private set; }
    public string EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }

    public string? Details { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? UserRole { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private ActivityLog() { }

    public static ActivityLog Create(
        Guid condominiumId,
        string action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string? oldValues = null,
        string? newValues = null,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? userRole = null)
    {
        return new ActivityLog
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            UserRole = userRole,
            CreatedAt = DateTime.UtcNow
        };
    }
}