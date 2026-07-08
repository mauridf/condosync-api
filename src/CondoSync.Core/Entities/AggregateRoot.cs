using CondoSync.Core.Events;

namespace CondoSync.Core.Entities;

public abstract class AggregateRoot<TId>
{
    private readonly List<DomainEvent> _domainEvents = new();

    public TId Id { get; protected set; } = default!;
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not AggregateRoot<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id?.Equals(other.Id) ?? false;
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(AggregateRoot<TId> left, AggregateRoot<TId> right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AggregateRoot<TId> left, AggregateRoot<TId> right)
    {
        return !Equals(left, right);
    }
}

// Interface para entidades que pertencem a um tenant
public interface ITenantEntity
{
    Guid CondominiumId { get; }
}

// Classe base para entidades com soft delete
public abstract class SoftDeletableEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}