using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class CleanupExpiredGuestListsJob : IJob
{
    private readonly IRepository<GuestList> _guestListRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CleanupExpiredGuestListsJob> _logger;

    public CleanupExpiredGuestListsJob(
        IRepository<GuestList> guestListRepo,
        IUnitOfWork unitOfWork,
        ILogger<CleanupExpiredGuestListsJob> logger)
    {
        _guestListRepo = guestListRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiredLists = await _guestListRepo.FindAsync(g =>
            g.EventDate < today && g.Status == "Active");

        var cancelledCount = 0;

        foreach (var list in expiredLists)
        {
            try
            {
                list.Cancel();
                cancelledCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao cancelar lista de convidados {GuestListId}", list.Id);
            }
        }

        if (cancelledCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Listas expiradas: {Count} listas canceladas", cancelledCount);
        }
    }
}
