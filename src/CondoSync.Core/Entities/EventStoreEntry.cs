namespace CondoSync.Core.Entities;

public class EventStoreEntry
{
    public long Id { get; private set; }
    public string AggregateType { get; private set; }
    public Guid AggregateId { get; private set; }
    public int Version { get; private set; }
    public string EventType { get; private set; }
    public string EventData { get; private set; }
    public string? Metadata { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private EventStoreEntry() { }

    public static EventStoreEntry Create(
        string aggregateType,
        Guid aggregateId,
        int version,
        string eventType,
        string eventData,
        string? metadata = null)
    {
        return new EventStoreEntry
        {
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Version = version,
            EventType = eventType,
            EventData = eventData,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };
    }
}