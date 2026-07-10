namespace CondoSync.Application.Features.Invitations.DTOs;

public record CreateInvitationRequest(
    Guid UnitId,
    string? RecipientEmail = null,
    string? RecipientName = null,
    string? RecipientPhone = null,
    string AccessType = "owner",
    int MaxUses = 1,
    int ValidityDays = 7
);

public record InvitationResponse(
    Guid Id,
    Guid UnitId,
    string InvitationCode,
    string? InvitationUrl,
    string? RecipientEmail,
    string? RecipientName,
    string? RecipientPhone,
    string AccessType,
    string Status,
    int MaxUses,
    int UsesCount,
    DateTime? ExpiresAt,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UseInvitationRequest(
    string InvitationCode
);
