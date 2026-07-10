namespace CondoSync.Application.Features.Bookings.DTOs;

public record CreateBookingRequest(
    Guid ServiceId,
    Guid UnitId,
    DateTime BookingDate,
    string StartTime,
    string EndTime,
    string? Title = null,
    string? Description = null,
    int GuestsCount = 0,
    string? SpecialRequirements = null
);

public record BookingResponse(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    Guid UnitId,
    string UnitNumber,
    Guid ResidentId,
    string ResidentName,
    DateOnly BookingDate,
    string StartTime,
    string EndTime,
    string Status,
    string? Title,
    int GuestsCount,
    decimal? Amount,
    string? PaymentStatus,
    DateTime CreatedAt
);

public record BookingDetailResponse(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    Guid UnitId,
    string UnitNumber,
    Guid ResidentId,
    string ResidentName,
    DateOnly BookingDate,
    string StartTime,
    string EndTime,
    string Status,
    string? Title,
    string? Description,
    int GuestsCount,
    string? SpecialRequirements,
    decimal? Amount,
    string? PaymentStatus,
    string? PaymentMethod,
    string? QrCodeUrl,
    DateTime? CheckedInAt,
    DateTime? CheckedOutAt,
    string? RejectionReason,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ApproveRejectRequest(
    string? Reason = null
);

public record CancelBookingRequest(
    string Reason
);

public record CalendarQueryParams(
    Guid? ServiceId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);