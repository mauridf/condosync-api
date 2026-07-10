using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class CompletePastBookingsJob : IJob
{
    private readonly IRepository<Booking> _bookingRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePastBookingsJob> _logger;

    public CompletePastBookingsJob(
        IRepository<Booking> bookingRepo,
        IUnitOfWork unitOfWork,
        ILogger<CompletePastBookingsJob> logger)
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
            b.CheckedInAt != null &&
            b.Status != BookingStatus.Completed &&
            b.Status != BookingStatus.Cancelled &&
            b.Status != BookingStatus.NoShow);

        var completedCount = 0;

        foreach (var booking in bookings)
        {
            try
            {
                booking.CheckOut();
                completedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao finalizar reserva {BookingId}", booking.Id);
            }
        }

        if (completedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Checkout: {Count} reservas finalizadas automaticamente", completedCount);
        }
    }
}
