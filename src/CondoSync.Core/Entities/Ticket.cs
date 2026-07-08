using CondoSync.Core.Enums;
using CondoSync.Core.Events;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Ticket : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid ResidentId { get; private set; }
    public Guid? AssignedTo { get; private set; }

    // Identificação
    public string TicketNumber { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    public string? Subcategory { get; private set; }

    // Prioridade
    public TicketPriority Priority { get; private set; }

    // Status
    public TicketStatus Status { get; private set; }

    // SLA
    public int SlaHours { get; private set; }
    public DateTime? SlaBreachedAt { get; private set; }

    // Resolução
    public string? Resolution { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ResolvedBy { get; private set; }

    // Localização
    public string? LocationType { get; private set; }
    public string? LocationDescription { get; private set; }

    // Avaliação
    public int? Rating { get; private set; }
    public string? Feedback { get; private set; }

    // Custo
    public decimal? Cost { get; private set; }
    public string? PaidBy { get; private set; }

    // Arquivos
    public string? Attachments { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Ticket() { }

    public static Ticket Create(
        Guid condominiumId,
        Guid unitId,
        Guid residentId,
        string title,
        string description,
        string category,
        TicketPriority priority = TicketPriority.Normal,
        string? subcategory = null,
        int slaHours = 48)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título do chamado não pode ser vazio");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Descrição do chamado não pode ser vazia");

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UnitId = unitId,
            ResidentId = residentId,
            Title = title,
            Description = description,
            Category = category,
            Subcategory = subcategory,
            Priority = priority,
            Status = TicketStatus.Open,
            SlaHours = slaHours,
            TicketNumber = GenerateTicketNumber(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        ticket.AddDomainEvent(new TicketOpenedEvent(ticket.Id, unitId, category, priority.ToString()));

        return ticket;
    }

    private static string GenerateTicketNumber()
    {
        return $"TCK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
    }

    public void AssignTo(Guid assignedTo)
    {
        AssignedTo = assignedTo;

        if (Status == TicketStatus.Open)
        {
            Status = TicketStatus.InProgress;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(TicketStatus newStatus, Guid? resolvedBy = null, string? resolution = null)
    {
        Status = newStatus;

        switch (newStatus)
        {
            case TicketStatus.Resolved:
                ResolvedAt = DateTime.UtcNow;
                ResolvedBy = resolvedBy;
                Resolution = resolution;
                AddDomainEvent(new TicketResolvedEvent(Id, resolvedBy ?? Guid.Empty));
                break;
            case TicketStatus.Closed:
                ClosedAt = DateTime.UtcNow;
                break;
            case TicketStatus.InProgress:
                if (Status == TicketStatus.Open && !AssignedTo.HasValue)
                    throw new DomainException("Chamado precisa ser atribuído antes de iniciar");
                break;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(TicketPriority newPriority)
    {
        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage(Guid senderId, string message, bool isInternal = false)
    {
        // A mensagem é adicionada via TicketMessage separadamente
        UpdatedAt = DateTime.UtcNow;
    }

    public void Evaluate(int rating, string? feedback = null)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Avaliação deve ser entre 1 e 5");

        if (Status != TicketStatus.Resolved && Status != TicketStatus.Closed)
            throw new DomainException("Só é possível avaliar chamados resolvidos ou fechados");

        Rating = rating;
        Feedback = feedback;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        if (Status != TicketStatus.Closed && Status != TicketStatus.Resolved)
            throw new DomainException("Apenas chamados fechados ou resolvidos podem ser reabertos");

        if (ClosedAt.HasValue && (DateTime.UtcNow - ClosedAt.Value).TotalDays > 7)
            throw new DomainException("Chamado só pode ser reaberto em até 7 dias após fechamento");

        Status = TicketStatus.Reopened;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CheckSla()
    {
        if (Status == TicketStatus.Open &&
            (DateTime.UtcNow - CreatedAt).TotalHours > SlaHours)
        {
            SlaBreachedAt = DateTime.UtcNow;
        }
    }
}