using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly IRepository<OutboxMessage> _outboxRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessOutboxMessagesJob> _logger;

    public ProcessOutboxMessagesJob(
        IRepository<OutboxMessage> outboxRepo,
        IUnitOfWork unitOfWork,
        ILogger<ProcessOutboxMessagesJob> logger)
    {
        _outboxRepo = outboxRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var messages = await _outboxRepo.FindAsync(m =>
            m.Status == "pending" && m.RetryCount < m.MaxRetries);

        var processedCount = 0;

        foreach (var message in messages)
        {
            try
            {
                message.MarkAsSent();
                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar outbox message {MessageId} do tipo {Type}",
                    message.Id, message.Type);
                message.MarkAsFailed(ex.Message, ex.StackTrace);
            }
        }

        if (processedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Outbox: {Count} mensagens processadas", processedCount);
        }
    }
}
