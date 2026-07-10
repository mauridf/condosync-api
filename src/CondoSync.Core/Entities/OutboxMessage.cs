namespace CondoSync.Core.Entities;

public class OutboxMessage : AggregateRoot<Guid>
{
    public string Type { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public string? Headers { get; private set; }

    public string Status { get; private set; } = default!;
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? LastError { get; private set; }
    public string? ErrorStackTrace { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string type, string content, string? headers = null, int maxRetries = 5)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Content = content,
            Headers = headers,
            Status = "pending",
            MaxRetries = maxRetries,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsSent()
    {
        Status = "sent";
        SentAt = DateTime.UtcNow;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error, string? stackTrace = null)
    {
        RetryCount++;
        LastError = error;
        ErrorStackTrace = stackTrace;

        if (RetryCount >= MaxRetries)
        {
            Status = "failed";
        }
        else
        {
            Status = "pending"; // Volta para fila para retry
        }
    }
}