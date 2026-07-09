using Microsoft.EntityFrameworkCore;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure.Services;

public class AdminDashboardService
{
    private readonly CondoSyncDbContext _condoSyncContext;
    private readonly AdminDbContext _adminContext;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(
        CondoSyncDbContext condoSyncContext,
        AdminDbContext adminContext,
        ILogger<AdminDashboardService> logger)
    {
        _condoSyncContext = condoSyncContext;
        _adminContext = adminContext;
        _logger = logger;
    }

    public async Task<object> GetSummaryAsync()
    {
        var totalCondominiums = await _condoSyncContext.Condominiums.CountAsync();
        var activeCondominiums = await _condoSyncContext.Condominiums
            .CountAsync(c => c.IsActive && c.SubscriptionStatus == SubscriptionStatus.Active);
        var trialCondominiums = await _condoSyncContext.Condominiums
            .CountAsync(c => c.SubscriptionPlan == SubscriptionPlan.Trial);
        var suspendedCondominiums = await _condoSyncContext.Condominiums
            .CountAsync(c => c.SubscriptionStatus == SubscriptionStatus.Suspended);
        var cancelledCondominiums = await _condoSyncContext.Condominiums
            .CountAsync(c => c.SubscriptionStatus == SubscriptionStatus.Cancelled);

        var totalUsers = await _condoSyncContext.Users.CountAsync();
        var totalUnits = await _condoSyncContext.Units.CountAsync();
        var totalResidents = await _condoSyncContext.Residents.CountAsync();
        var totalBookings = await _condoSyncContext.Bookings.CountAsync();
        var totalTickets = await _condoSyncContext.Tickets.CountAsync();
        var openTickets = await _condoSyncContext.Tickets
            .CountAsync(t => t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var newCondominiumsLast30Days = await _condoSyncContext.Condominiums
            .CountAsync(c => c.CreatedAt >= thirtyDaysAgo);

        var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);
        var trialsExpiringSoon = await _condoSyncContext.Condominiums
            .CountAsync(c => c.SubscriptionPlan == SubscriptionPlan.Trial
                && c.TrialEndsAt != null
                && c.TrialEndsAt <= sevenDaysFromNow
                && c.TrialEndsAt >= DateTime.UtcNow);

        return new
        {
            TotalCondominiums = totalCondominiums,
            ActiveCondominiums = activeCondominiums,
            TrialCondominiums = trialCondominiums,
            SuspendedCondominiums = suspendedCondominiums,
            CancelledCondominiums = cancelledCondominiums,
            TotalUsers = totalUsers,
            TotalUnits = totalUnits,
            TotalResidents = totalResidents,
            TotalBookings = totalBookings,
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            NewCondominiumsLast30Days = newCondominiumsLast30Days,
            TrialsExpiringSoon = trialsExpiringSoon,
            Timestamp = DateTime.UtcNow
        };
    }

    public async Task<object> GetSubscriptionDistributionAsync()
    {
        var distribution = await _condoSyncContext.Condominiums
            .GroupBy(c => c.SubscriptionPlan)
            .Select(g => new
            {
                Plan = g.Key.ToString(),
                Count = g.Count(),
                Percentage = Math.Round((double)g.Count() /
                    Math.Max(1, _condoSyncContext.Condominiums.Count()) * 100, 1)
            })
            .ToListAsync();

        return new
        {
            Distribution = distribution,
            Total = distribution.Sum(d => d.Count)
        };
    }

    public async Task<object> GetGrowthMetricsAsync()
    {
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-12);

        var monthlyGrowth = await _condoSyncContext.Condominiums
            .Where(c => c.CreatedAt >= twelveMonthsAgo)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                NewCondominiums = g.Count(),
                ActiveCount = g.Count(c => c.IsActive)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync();

        var totalNow = await _condoSyncContext.Condominiums.CountAsync();
        var total12MonthsAgo = await _condoSyncContext.Condominiums
            .CountAsync(c => c.CreatedAt < twelveMonthsAgo);

        var growthPercentage = total12MonthsAgo > 0
            ? Math.Round(((double)(totalNow - total12MonthsAgo) / total12MonthsAgo) * 100, 2)
            : 100.0;

        return new
        {
            MonthlyGrowth = monthlyGrowth,
            TotalCondominiumsNow = totalNow,
            TotalCondominiums12MonthsAgo = total12MonthsAgo,
            GrowthPercentage = growthPercentage
        };
    }

    public async Task<object> GetChurnRateAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var cancelledCondominiums = await _condoSyncContext.Condominiums
            .Where(c => c.DeletedAt >= sixMonthsAgo ||
                (c.SubscriptionStatus == SubscriptionStatus.Cancelled && c.UpdatedAt >= sixMonthsAgo))
            .CountAsync();

        var averageActive = await _condoSyncContext.Condominiums
            .Where(c => c.IsActive && c.SubscriptionStatus == SubscriptionStatus.Active)
            .CountAsync();

        var churnRate = averageActive > 0
            ? Math.Round(((double)cancelledCondominiums / averageActive) * 100, 2)
            : 0.0;

        return new
        {
            CancelledLast6Months = cancelledCondominiums,
            AverageActiveCondominiums = averageActive,
            ChurnRatePercentage = churnRate,
            Period = "Últimos 6 meses"
        };
    }

    public async Task<object> GetRecentCondominiumsAsync(int count = 10)
    {
        var recent = await _condoSyncContext.Condominiums
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Email,
                c.SubscriptionPlan,
                c.SubscriptionStatus,
                c.IsActive,
                c.CreatedAt
            })
            .ToListAsync();

        return new { RecentCondominiums = recent };
    }

    public async Task<object> GetSystemStatsAsync()
    {
        var totalAdmins = await _adminContext.SuperAdmins.CountAsync();

        var ticketsByStatus = await _condoSyncContext.Tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var bookingsByStatus = await _condoSyncContext.Bookings
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var pendingBills = await _condoSyncContext.Bills
            .CountAsync(b => b.Status == BillStatus.Pending);
        var overdueBills = await _condoSyncContext.Bills
            .CountAsync(b => b.Status == BillStatus.Overdue);

        return new
        {
            TotalAdmins = totalAdmins,
            TicketsByStatus = ticketsByStatus,
            BookingsByStatus = bookingsByStatus,
            PendingBills = pendingBills,
            OverdueBills = overdueBills,
            TotalNotifications = await _condoSyncContext.Notifications.CountAsync()
        };
    }
}