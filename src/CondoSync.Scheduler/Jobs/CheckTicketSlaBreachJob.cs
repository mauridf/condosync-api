using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class CheckTicketSlaBreachJob : IJob
{
    private readonly IRepository<Ticket> _ticketRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CheckTicketSlaBreachJob> _logger;

    public CheckTicketSlaBreachJob(
        IRepository<Ticket> ticketRepo,
        IUnitOfWork unitOfWork,
        ILogger<CheckTicketSlaBreachJob> logger)
    {
        _ticketRepo = ticketRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;

        var tickets = await _ticketRepo.FindAsync(t =>
            t.Status == TicketStatus.Open &&
            t.SlaBreachedAt == null &&
            t.SlaHours > 0);

        var breachedCount = 0;

        foreach (var ticket in tickets)
        {
            try
            {
                ticket.CheckSla();
                if (ticket.SlaBreachedAt != null)
                    breachedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao verificar SLA do ticket {TicketId}", ticket.Id);
            }
        }

        if (breachedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogWarning("SLA: {Count} tickets com SLA violado", breachedCount);
        }
    }
}
