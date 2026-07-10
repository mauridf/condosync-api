using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ExpirePastBookingsJob : IJob
{
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpirePastBookingsJob> _logger;

    public ExpirePastBookingsJob(
        IRepository<Booking> bookingRepo,
        IUnitOfWork unitOfWork,
        ILogger<ExpirePastBookingsJob> logger)
    {
        _bookingRepo = bookingRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bookings = await _bookingRepo.FindAsync(b =>
            b.BookingDate < today &&
            b.Status == BookingStatus.Approved &&
            b.CheckedInAt == null);

        var expiredCount = 0;

        foreach (var booking in bookings)
        {
            try
            {
                booking.MarkAsNoShow();
                expiredCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao marcar no-show para reserva {BookingId}", booking.Id);
            }
        }

        if (expiredCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("NoShow: {Count} reservas marcadas como no-show", expiredCount);
        }
    }
}
