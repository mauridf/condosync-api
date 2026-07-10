namespace CondoSync.Application.Features.Dashboard.DTOs;

public record ResidentDashboardResponse(
    UnitInfo MyUnit,
    BillsSummary MyBills,
    TicketsSummary MyTickets,
    BookingsSummary MyBookings,
    int UnreadNotifications,
    int ActiveNotices,
    int ActivePolls
);

public record UnitInfo(
    Guid UnitId,
    string UnitNumber,
    string? Block,
    int ResidentCount
);

public record BillsSummary(
    int Pending,
    int Overdue,
    string? NextDueDate,
    decimal? NextAmount
);

public record TicketsSummary(
    int Open,
    int Total
);

public record BookingsSummary(
    int Upcoming,
    BookingItem? NextBooking
);

public record BookingItem(
    Guid Id,
    Guid ServiceId,
    string ServiceName,
    string BookingDate,
    string StartTime,
    string EndTime,
    string Status
);
