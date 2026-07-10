using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class TicketMessage : AggregateRoot<Guid>
{
    public Guid TicketId { get; private set; }
    public Guid SenderId { get; private set; }

    public string Message { get; private set; } = default!;
    public bool IsInternal { get; private set; }
    public bool IsSystemMessage { get; private set; }
    public string? Attachments { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private TicketMessage() { }

    public static TicketMessage Create(Guid ticketId, Guid senderId, string message,
        bool isInternal = false, bool isSystemMessage = false, string? attachments = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new DomainException("Mensagem não pode ser vazia");

        return new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SenderId = senderId,
            Message = message,
            IsInternal = isInternal,
            IsSystemMessage = isSystemMessage,
            Attachments = attachments,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static TicketMessage CreateSystemMessage(Guid ticketId, string message)
    {
        return new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            SenderId = Guid.Empty,
            Message = message,
            IsSystemMessage = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}