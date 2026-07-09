namespace CondoSync.Application.Features.Services.DTOs;

public record CreateServiceRequest(
    string Name,
    string Slug,
    string Category,
    string ServiceType,
    string? Description = null,
    decimal Price = 0,
    bool RequiresApproval = false,
    bool RequiresPayment = false,
    string? PriceUnit = null,
    int? MaxBookingPerDay = null,
    int? MaxBookingPerUser = null,
    int AdvanceBookingDays = 0,
    int CancelBeforeHours = 24,
    bool AllowSimultaneous = false,
    List<int>? AvailableDays = null,
    string? AvailableTimeStart = null,
    string? AvailableTimeEnd = null,
    int? SlotDuration = null,
    string? Rules = null,
    string? TermsOfUse = null
);

public record UpdateServiceRequest(
    string Name,
    string? Description = null,
    decimal? Price = null,
    bool? RequiresApproval = null,
    bool? IsActive = null
);

public record ServiceResponse(
    Guid Id,
    string Name,
    string Slug,
    string Category,
    string ServiceType,
    string? Description,
    string? Icon,
    decimal Price,
    string? PriceUnit,
    bool RequiresApproval,
    bool RequiresPayment,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt
);