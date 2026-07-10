namespace CondoSync.Application.Features.Notifications.DTOs;

public record CreateNotificationTemplateRequest(
    string Name,
    string TitleTemplate,
    string BodyTemplate,
    string NotificationType,
    string Channel = "in_app",
    string? Description = null
);

public record UpdateNotificationTemplateRequest(
    string? Name,
    string? TitleTemplate,
    string? BodyTemplate,
    string? Description
);

public record NotificationTemplateResponse(
    Guid Id,
    string Name,
    string? Description,
    string TitleTemplate,
    string BodyTemplate,
    string NotificationType,
    string Channel,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
