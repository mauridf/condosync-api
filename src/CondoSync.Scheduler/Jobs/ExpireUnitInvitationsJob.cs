using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ExpireUnitInvitationsJob : IJob
{
    private readonly IRepository<UnitInvitation> _invitationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExpireUnitInvitationsJob> _logger;

    public ExpireUnitInvitationsJob(
        IRepository<UnitInvitation> invitationRepo,
        IUnitOfWork unitOfWork,
        ILogger<ExpireUnitInvitationsJob> logger)
    {
        _invitationRepo = invitationRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;

        var invitations = await _invitationRepo.FindAsync(i =>
            i.Status == "active" &&
            i.ExpiresAt != null &&
            i.ExpiresAt <= now);

        var expiredCount = 0;

        foreach (var invitation in invitations)
        {
            try
            {
                invitation.Expire();
                expiredCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao expirar convite {InvitationId}", invitation.Id);
            }
        }

        if (expiredCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Convites: {Count} convites expirados", expiredCount);
        }
    }
}
