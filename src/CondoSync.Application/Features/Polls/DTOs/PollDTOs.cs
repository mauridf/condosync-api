namespace CondoSync.Application.Features.Polls.DTOs;

public record CreatePollRequest(
    string Title,
    List<PollOption> Options,
    DateTime StartsAt,
    DateTime EndsAt,
    string PollType = "Single",
    bool IsAnonymous = false,
    bool RequiresUnitVote = false,
    string? Description = null,
    string? VotingRule = null,
    bool IsBinding = false,
    string? VoterSlug = null
);

public record VoteRequest(List<Guid> SelectedOptions);

public record PollOption(Guid Id, string Text, int Order);

public record TallyResult(
    Guid PollId,
    string Title,
    int TotalVotes,
    string Status,
    string PollCategory,
    string? VotingRule,
    bool IsBinding,
    Guid? ElectedCandidateId,
    Guid? ApprovedOptionId,
    string? VoterSlug,
    List<OptionResult> Results
);

public record OptionResult(
    Guid OptionId,
    string OptionText,
    int VoteCount,
    decimal Percentage,
    bool IsWinning
);
