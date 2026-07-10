namespace CondoSync.Application.Features.Visitors.DTOs;

public record AuthorizeVisitorRequest(
    Guid UnitId,
    string Name,
    DateTime VisitDate,
    string VisitorType = "Guest",
    Guid? ResidentId = null,
    string? Phone = null,
    string? CompanyName = null,
    string? Notes = null
);
