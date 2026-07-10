namespace CondoSync.Application.Features.CommonAreas.DTOs;

public record CreateCommonAreaRequest(
    string Name,
    string Type,
    string? Description = null,
    int? Capacity = null,
    bool RequiresBooking = false
);

public record UpdateCommonAreaRequest(
    string? Name,
    string? Description = null,
    int? Capacity = null,
    bool? RequiresBooking = null
);

public record CommonAreaResponse(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    int? Capacity,
    int MaxGuestsPerResident,
    bool RequiresBooking,
    bool RequiresDeposit,
    decimal? DepositAmount,
    string? OperatingHours,
    bool IsActive,
    string MaintenanceStatus,
    string? ScheduledMaintenance,
    string? Images,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
