namespace CondoSync.Application.Features.Tickets.DTOs;

public record CreateTicketRequest(
    Guid UnitId,
    Guid ResidentId,
    string Title,
    string Description,
    string Category,
    string Priority = "Normal",
    string? Subcategory = null
);

public record UpdateTicketStatusRequest(
    string Status,
    Guid? ResolvedBy = null,
    string? Resolution = null
);

public record AddTicketMessageRequest(
    string Message,
    bool IsInternal = false
);
