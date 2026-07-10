using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ArchiveOldActivityLogsJob : IJob
{
    private readonly IRepository<ActivityLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ArchiveOldActivityLogsJob> _logger;

    public ArchiveOldActivityLogsJob(
        IRepository<ActivityLog> logRepo,
        IUnitOfWork unitOfWork,
        ILogger<ArchiveOldActivityLogsJob> logger)
    {
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = DateTime.UtcNow.AddDays(-90);

        var oldLogs = await _logRepo.FindAsync(l => l.CreatedAt < cutoff);

        var count = oldLogs.Count();

        if (count > 0)
        {
            _logRepo.RemoveRange(oldLogs);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Logs: {Count} logs mais antigos que 90 dias removidos", count);
        }
    }
}
