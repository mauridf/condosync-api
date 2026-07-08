using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class PollVote : AggregateRoot<Guid>
{
    public Guid PollId { get; private set; }
    public Guid? UnitId { get; private set; }
    public Guid? ResidentId { get; private set; }
    public Guid? UserId { get; private set; }

    public List<Guid> SelectedOptions { get; private set; }

    public DateTime VotedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private PollVote() { }

    public static PollVote Create(
        Guid pollId,
        List<Guid> selectedOptions,
        Guid? unitId = null,
        Guid? residentId = null,
        Guid? userId = null)
    {
        if (selectedOptions == null || selectedOptions.Count == 0)
            throw new DomainException("Pelo menos uma opção deve ser selecionada");

        return new PollVote
        {
            Id = Guid.NewGuid(),
            PollId = pollId,
            SelectedOptions = selectedOptions,
            UnitId = unitId,
            ResidentId = residentId,
            UserId = userId,
            VotedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateVote(List<Guid> newSelectedOptions)
    {
        if (newSelectedOptions == null || newSelectedOptions.Count == 0)
            throw new DomainException("Pelo menos uma opção deve ser selecionada");

        SelectedOptions = newSelectedOptions;
        UpdatedAt = DateTime.UtcNow;
    }
}