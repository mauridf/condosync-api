using CondoSync.Core.Enums;

namespace CondoSync.Application.Features.Visitors.DTOs;

public record AuthorizeVisitorRequest(
    Guid UnitId,
    string Name,
    DateTime VisitDate,
    string VisitorType = "Guest",
    Guid? ResidentId = null,
    string? Phone = null,
    string? CompanyName = null,
    string? Notes = null,
    string? Document = null,
    string? DocumentType = null,
    string? VehiclePlate = null,
    string? VehicleModel = null,
    TimeOnly? ExpectedEntryTime = null,
    TimeOnly? ExpectedExitTime = null,
    Guid? GuestListId = null
);

public record UpdateVisitorRequest(
    string Name,
    string? Phone = null,
    string? Notes = null
);

public record VisitorResponse(
    Guid Id,
    Guid UnitId,
    string Name,
    string VisitDate,
    string VisitorType,
    string Status,
    string? AuthorizationCode,
    string? Phone,
    string? CompanyName,
    string? Document,
    string? VehiclePlate,
    DateTime? EntryTime,
    DateTime? ExitTime,
    Guid? GuestListId,
    DateTime CreatedAt
);

public record VisitorDetailResponse(
    Guid Id,
    Guid CondominiumId,
    Guid UnitId,
    Guid? ResidentId,
    string Name,
    string VisitDate,
    string VisitorType,
    string Status,
    string? AuthorizationCode,
    string? Phone,
    string? CompanyName,
    string? Document,
    string? DocumentType,
    string? VehiclePlate,
    string? VehicleModel,
    TimeOnly? ExpectedEntryTime,
    TimeOnly? ExpectedExitTime,
    DateTime? EntryTime,
    DateTime? ExitTime,
    string? Notes,
    bool IsRecurring,
    Guid? GuestListId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateGuestListRequest(
    string Title,
    DateTime EventDate,
    Guid? BookingId = null,
    Guid? UnitId = null,
    string? Description = null,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int MaxGuests = 50,
    bool RequiresQrCode = true
);

public record UpdateGuestListRequest(
    string Title,
    string? Description = null,
    TimeOnly? StartTime = null,
    TimeOnly? EndTime = null,
    int MaxGuests = 50,
    bool RequiresQrCode = true
);

public record GuestListResponse(
    Guid Id,
    string Title,
    string? Description,
    string EventDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int MaxGuests,
    bool RequiresQrCode,
    string Status,
    int VisitorCount,
    DateTime CreatedAt
);
