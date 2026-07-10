namespace CondoSync.Application.Features.Polls.DTOs;

public record CreatePollRequest(
    string Title,
    List<PollOption> Options,
    DateTime StartsAt,
    DateTime EndsAt,
    string PollType = "Single",
    bool IsAnonymous = false,
    bool RequiresUnitVote = false,
    string? Description = null
);

public record VoteRequest(List<Guid> SelectedOptions);

public record PollOption(Guid Id, string Text, int Order);
