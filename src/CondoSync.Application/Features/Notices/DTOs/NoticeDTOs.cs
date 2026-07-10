namespace CondoSync.Application.Features.Notices.DTOs;

public record CreateNoticeRequest(
    string Title,
    string Content,
    string Category = "General",
    bool IsUrgent = false,
    string? Summary = null,
    string Visibility = "all"
);

public record UpdateNoticeRequest(
    string Title,
    string Content,
    string? Summary = null,
    string? Category = null,
    bool? IsUrgent = null
);

public record AddCommentRequest(string Content);

public record AddReactionRequest(string ReactionType);
