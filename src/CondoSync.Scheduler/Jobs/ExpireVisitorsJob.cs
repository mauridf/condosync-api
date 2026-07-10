using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ExpireVisitorsJob : IJob
{
    private readonly IRepository<Visitor> _visitorRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpireVisitorsJob> _logger;

    public ExpireVisitorsJob(
        IRepository<Visitor> visitorRepo,
        IUnitOfWork unitOfWork,
        ILogger<ExpireVisitorsJob> logger)
    {
        _visitorRepo = visitorRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expiredVisitors = await _visitorRepo.FindAsync(v =>
            v.VisitDate < today && v.Status == VisitorStatus.Authorized);

        var expiredCount = 0;

        foreach (var visitor in expiredVisitors)
        {
            try
            {
                visitor.Expire();
                expiredCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao expirar visitante {VisitorId}", visitor.Id);
            }
        }

        if (expiredCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Visitantes expirados: {Count} autorizações expiradas", expiredCount);
        }
    }
}
