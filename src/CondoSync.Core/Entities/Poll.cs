using CondoSync.Core.Enums;
using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class Poll : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid CreatedBy { get; private set; }

    public string Title { get; private set; }
    public string? Description { get; private set; }

    // Configuração
    public PollType PollType { get; private set; }
    public bool IsAnonymous { get; private set; }
    public bool IsMandatory { get; private set; }
    public bool RequiresUnitVote { get; private set; }

    // Opções
    public string Options { get; private set; }

    // Datas
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    // Resultados
    public int TotalVotes { get; private set; }
    public string ResultsVisibility { get; private set; }

    // Status
    public PollStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Poll() { }

    public static Poll Create(
        Guid condominiumId,
        Guid createdBy,
        string title,
        string options, // JSON array: [{"id":"opt1","text":"Sim"},...]
        DateTime startsAt,
        DateTime endsAt,
        PollType pollType = PollType.Single,
        bool isAnonymous = false,
        bool requiresUnitVote = false,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Título da enquete não pode ser vazio");

        if (startsAt >= endsAt)
            throw new DomainException("Data de início deve ser anterior ao fim");

        return new Poll
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            CreatedBy = createdBy,
            Title = title,
            Description = description,
            Options = options,
            PollType = pollType,
            IsAnonymous = isAnonymous,
            IsMandatory = false,
            RequiresUnitVote = requiresUnitVote,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = PollStatus.Draft,
            ResultsVisibility = "after_end",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Open()
    {
        if (Status != PollStatus.Draft)
            throw new DomainException("Apenas enquetes em rascunho podem ser abertas");

        Status = PollStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status != PollStatus.Active)
            throw new DomainException("Apenas enquetes ativas podem ser fechadas");

        Status = PollStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PollStatus.Closed)
            throw new DomainException("Enquete já está fechada");

        Status = PollStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordVote()
    {
        if (Status != PollStatus.Active)
            throw new DomainException("Enquete não está ativa");

        if (DateTime.UtcNow < StartsAt)
            throw new DomainException("Enquete ainda não começou");

        if (DateTime.UtcNow > EndsAt)
            throw new DomainException("Enquete já terminou");

        TotalVotes++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}