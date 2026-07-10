using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class CondoDashboardService
{
    private readonly IRepository<Unit> _unitRepository;
    private readonly IRepository<Resident> _residentRepository;
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Ticket> _ticketRepository;
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IRepository<Visitor> _visitorRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CondoDashboardService> _logger;

    public CondoDashboardService(
        IRepository<Unit> unitRepository,
        IRepository<Resident> residentRepository,
        IRepository<Bill> billRepository,
        IRepository<Ticket> ticketRepository,
        IRepository<Booking> bookingRepository,
        IRepository<Visitor> visitorRepository,
        ITenantProvider tenantProvider,
        ILogger<CondoDashboardService> logger)
    {
        _unitRepository = unitRepository;
        _residentRepository = residentRepository;
        _billRepository = billRepository;
        _ticketRepository = ticketRepository;
        _bookingRepository = bookingRepository;
        _visitorRepository = visitorRepository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<object> GetSummaryAsync()
    {
        var tenantId = GetCurrentTenantId();

        var units = await _unitRepository.FindAsync(u => u.CondominiumId == tenantId && u.IsActive);
        var unitsList = units.ToList();

        var totalUnits = unitsList.Count;
        var occupiedUnits = unitsList.Count(u => u.OccupancyStatus != UnitOccupancyStatus.Vacant);
        var occupancyRate = totalUnits > 0 ? Math.Round((double)occupiedUnits / totalUnits * 100, 1) : 0;

        var residents = await _residentRepository.FindAsync(r => r.CondominiumId == tenantId && r.IsActive);
        var totalResidents = residents.Count();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonth = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}";

        // Faturas do mês
        var bills = await _billRepository.FindAsync(b => b.CondominiumId == tenantId);
        var monthlyBills = bills.Where(b => b.ReferenceMonth == currentMonth);
        var totalBilled = monthlyBills.Sum(b => b.TotalAmount);
        var totalPaid = monthlyBills.Where(b => b.Status == BillStatus.Paid).Sum(b => b.PaymentAmount ?? 0);
        var pendingBills = monthlyBills.Count(b => b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue);

        // Tickets abertos
        var tickets = await _ticketRepository.FindAsync(t => t.CondominiumId == tenantId);
        var openTickets = tickets.Count(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);
        var urgentTickets = tickets.Count(t => t.Priority == TicketPriority.Urgent || t.Priority == TicketPriority.Critical);

        // Reservas do dia
        var bookings = await _bookingRepository.FindAsync(b => b.CondominiumId == tenantId);
        var todayBookings = bookings.Where(b => b.BookingDate == today && b.Status != BookingStatus.Cancelled).ToList();

        // Visitantes do dia
        var visitors = await _visitorRepository.FindAsync(v => v.CondominiumId == tenantId);
        var todayVisitors = visitors.Where(v => v.VisitDate == today).ToList();

        return new
        {
            TotalUnits = totalUnits,
            OccupiedUnits = occupiedUnits,
            OccupancyRate = occupancyRate,
            TotalResidents = totalResidents,
            MonthlyBilling = new
            {
                ReferenceMonth = currentMonth,
                TotalBilled = totalBilled,
                TotalPaid = totalPaid,
                PendingBills = pendingBills
            },
            Tickets = new
            {
                OpenTickets = openTickets,
                UrgentTickets = urgentTickets
            },
            TodayBookings = todayBookings.Count,
            TodayVisitors = todayVisitors.Count,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<object> GetRecentActivityAsync(int count = 10)
    {
        var tenantId = GetCurrentTenantId();

        var bookings = await _bookingRepository.FindAsync(b => b.CondominiumId == tenantId);
        var recentBookings = bookings
            .OrderByDescending(b => b.CreatedAt)
            .Take(5)
            .Select(b => new { Type = "booking", b.Id, b.BookingDate, b.Status, b.CreatedAt });

        var tickets = await _ticketRepository.FindAsync(t => t.CondominiumId == tenantId);
        var recentTickets = tickets
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .Select(t => new { Type = "ticket", t.Id, t.TicketNumber, t.Title, t.Status, t.CreatedAt });

        var visitors = await _visitorRepository.FindAsync(v => v.CondominiumId == tenantId);
        var recentVisitors = visitors
            .OrderByDescending(v => v.CreatedAt)
            .Take(5)
            .Select(v => new { Type = "visitor", v.Id, v.Name, v.VisitDate, v.Status, v.CreatedAt });

        var activities = recentBookings
            .Cast<object>()
            .Concat(recentTickets.Cast<object>())
            .Concat(recentVisitors.Cast<object>())
            .OrderByDescending(a => ((dynamic)a).CreatedAt)
            .Take(count);

        return new { Activities = activities };
    }

    public async Task<object> GetAdvancedStatsAsync()
    {
        var tenantId = GetCurrentTenantId();

        var tickets = await _ticketRepository.FindAsync(t => t.CondominiumId == tenantId);
        var ticketList = tickets.ToList();
        var ticketCategories = ticketList
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        var bookings = await _bookingRepository.FindAsync(b => b.CondominiumId == tenantId);
        var bookingList = bookings.ToList();
        var bookingsByMonth = bookingList
            .GroupBy(b => $"{b.BookingDate.Year}-{b.BookingDate.Month:D2}")
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .OrderBy(x => x.Month);

        var bills = await _billRepository.FindAsync(b => b.CondominiumId == tenantId);
        var billList = bills.ToList();
        var paymentRate = billList.Any()
            ? Math.Round((double)billList.Count(b => b.Status == BillStatus.Paid) / billList.Count * 100, 1)
            : 0;

        var overdueRate = billList.Any()
            ? Math.Round((double)billList.Count(b => b.Status == BillStatus.Overdue) / billList.Count * 100, 1)
            : 0;

        var totalBilled = billList.Sum(b => b.TotalAmount);
        var totalReceived = billList.Where(b => b.Status == BillStatus.Paid).Sum(b => b.PaymentAmount ?? 0);
        var receiptRate = totalBilled > 0 ? Math.Round((double)totalReceived / (double)totalBilled * 100, 1) : 0;

        var violations = bookingList
            .GroupBy(b => b.ServiceId)
            .Select(g => new { ServiceId = g.Key, NoShowCount = g.Count(b => b.Status == BookingStatus.NoShow) })
            .OrderByDescending(x => x.NoShowCount);

        return new
        {
            TicketCategories = ticketCategories,
            BookingsByMonth = bookingsByMonth,
            Financial = new
            {
                PaymentRate = paymentRate,
                OverdueRate = overdueRate,
                TotalBilled = totalBilled,
                TotalReceived = totalReceived,
                ReceiptRate = receiptRate
            },
            NoShowViolations = violations,
            Timestamp = DateTime.UtcNow
        };
    }
}