using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Dashboard.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class ResidentDashboardService
{
    private readonly IRepository<Resident> _residentRepo;
    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<Bill> _billRepo;
    private readonly IRepository<Ticket> _ticketRepo;
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IRepository<Service> _serviceRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IRepository<Notice> _noticeRepo;
    private readonly IRepository<Poll> _pollRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<ResidentDashboardService> _logger;

    public ResidentDashboardService(
        IRepository<Resident> residentRepo,
        IRepository<Unit> unitRepo,
        IRepository<Bill> billRepo,
        IRepository<Ticket> ticketRepo,
        IRepository<Booking> bookingRepo,
        IRepository<Service> serviceRepo,
        IRepository<Notification> notificationRepo,
        IRepository<Notice> noticeRepo,
        IRepository<Poll> pollRepo,
        ITenantProvider tenantProvider,
        ILogger<ResidentDashboardService> logger)
    {
        _residentRepo = residentRepo;
        _unitRepo = unitRepo;
        _billRepo = billRepo;
        _ticketRepo = ticketRepo;
        _bookingRepo = bookingRepo;
        _serviceRepo = serviceRepo;
        _notificationRepo = notificationRepo;
        _noticeRepo = noticeRepo;
        _pollRepo = pollRepo;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<ResidentDashboardResponse> GetResidentDashboardAsync(Guid userId)
    {
        var tenantId = GetCurrentTenantId();

        var residents = await _residentRepo.FindAsync(r =>
            r.UserId == userId && r.CondominiumId == tenantId && r.IsActive);
        var resident = residents.FirstOrDefault();

        if (resident == null)
            throw new InvalidOperationException("RESIDENT_NOT_FOUND");

        var unitId = resident.UnitId;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var unitTask = GetUnitInfoAsync(tenantId, unitId);
        var billsTask = GetBillsSummaryAsync(tenantId, unitId);
        var ticketsTask = GetTicketsSummaryAsync(tenantId, unitId);
        var bookingsTask = GetBookingsSummaryAsync(tenantId, unitId, today);
        var notificationsTask = GetUnreadNotificationsAsync(tenantId, userId);
        var noticesTask = GetActiveNoticesCountAsync(tenantId);
        var pollsTask = GetActivePollsCountAsync(tenantId);
        var residentCountTask = GetResidentCountAsync(tenantId, unitId);

        await Task.WhenAll(
            unitTask, billsTask, ticketsTask, bookingsTask,
            notificationsTask, noticesTask, pollsTask, residentCountTask);

        var unitInfo = unitTask.Result;
        var residentCount = residentCountTask.Result;

        return new ResidentDashboardResponse(
            new UnitInfo(unitId, unitInfo.Number, unitInfo.Block, residentCount),
            billsTask.Result,
            ticketsTask.Result,
            bookingsTask.Result,
            notificationsTask.Result,
            noticesTask.Result,
            pollsTask.Result
        );
    }

    private async Task<(string Number, string? Block)> GetUnitInfoAsync(Guid tenantId, Guid unitId)
    {
        var units = await _unitRepo.FindAsync(u => u.Id == unitId && u.CondominiumId == tenantId);
        var unit = units.FirstOrDefault();
        return unit != null ? (unit.Number, unit.Block) : ("—", null);
    }

    private async Task<BillsSummary> GetBillsSummaryAsync(Guid tenantId, Guid unitId)
    {
        var bills = await _billRepo.FindAsync(b =>
            b.UnitId == unitId && b.CondominiumId == tenantId);
        var list = bills.ToList();

        var pending = list.Count(b => b.Status == BillStatus.Pending);
        var overdue = list.Count(b => b.Status == BillStatus.Overdue);

        var next = list
            .Where(b => b.Status is BillStatus.Pending or BillStatus.Overdue)
            .OrderBy(b => b.DueDate)
            .Select(b => new { b.DueDate, b.TotalAmount })
            .FirstOrDefault();

        return new BillsSummary(
            pending,
            overdue,
            next?.DueDate.ToString("yyyy-MM-dd"),
            next?.TotalAmount
        );
    }

    private async Task<TicketsSummary> GetTicketsSummaryAsync(Guid tenantId, Guid unitId)
    {
        var tickets = await _ticketRepo.FindAsync(t =>
            t.UnitId == unitId && t.CondominiumId == tenantId);
        var list = tickets.ToList();

        var open = list.Count(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.Reopened);

        return new TicketsSummary(open, list.Count);
    }

    private async Task<BookingsSummary> GetBookingsSummaryAsync(Guid tenantId, Guid unitId, DateOnly today)
    {
        var bookings = await _bookingRepo.FindAsync(b =>
            b.UnitId == unitId && b.CondominiumId == tenantId);
        var list = bookings.ToList();

        var upcoming = list
            .Where(b => b.BookingDate >= today && b.Status is not (BookingStatus.Cancelled or BookingStatus.Rejected))
            .OrderBy(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .ToList();

        var services = await _serviceRepo.FindAsync(s => s.CondominiumId == tenantId);
        var servicesDict = services.ToDictionary(s => s.Id, s => s.Name);

        BookingItem? next = null;
        if (upcoming.Count != 0)
        {
            var first = upcoming[0];
            next = new BookingItem(
                first.Id,
                first.ServiceId,
                servicesDict.GetValueOrDefault(first.ServiceId, "—"),
                first.BookingDate.ToString("yyyy-MM-dd"),
                first.StartTime.ToString(@"hh\:mm"),
                first.EndTime.ToString(@"hh\:mm"),
                first.Status.ToString()
            );
        }

        return new BookingsSummary(upcoming.Count, next);
    }

    private async Task<int> GetUnreadNotificationsAsync(Guid tenantId, Guid userId)
    {
        var notifications = await _notificationRepo.FindAsync(n =>
            n.UserId == userId && n.CondominiumId == tenantId);
        return notifications.Count(n => n.ReadAt == null);
    }

    private async Task<int> GetActiveNoticesCountAsync(Guid tenantId)
    {
        var notices = await _noticeRepo.FindAsync(n => n.CondominiumId == tenantId);
        return notices.Count(n =>
            n.PublishedAt != null &&
            (n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow));
    }

    private async Task<int> GetActivePollsCountAsync(Guid tenantId)
    {
        var polls = await _pollRepo.FindAsync(p => p.CondominiumId == tenantId);
        return polls.Count(p => p.Status == PollStatus.Active);
    }

    private async Task<int> GetResidentCountAsync(Guid tenantId, Guid unitId)
    {
        var residents = await _residentRepo.FindAsync(r =>
            r.UnitId == unitId && r.CondominiumId == tenantId && r.IsActive);
        return residents.Count();
    }
}
