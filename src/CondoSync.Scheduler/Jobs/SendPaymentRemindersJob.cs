using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class SendPaymentRemindersJob : IJob
{
    private readonly IRepository<Bill> _billRepo;
    private readonly IRepository<Resident> _residentRepo;
    private readonly IRepository<Notification> _notificationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendPaymentRemindersJob> _logger;

    public SendPaymentRemindersJob(
        IRepository<Bill> billRepo,
        IRepository<Resident> residentRepo,
        IRepository<Notification> notificationRepo,
        IUnitOfWork unitOfWork,
        ILogger<SendPaymentRemindersJob> logger)
    {
        _billRepo = billRepo;
        _residentRepo = residentRepo;
        _notificationRepo = notificationRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var pendingBills = await _billRepo.FindAsync(b =>
            (b.Status == BillStatus.Pending || b.Status == BillStatus.Overdue) &&
            b.DueDate <= today.AddDays(3));

        var reminderCount = 0;

        foreach (var bill in pendingBills)
        {
            try
            {
                var residents = await _residentRepo.FindAsync(r =>
                    r.UnitId == bill.UnitId && r.IsActive);

                foreach (var resident in residents)
                {
                    var userId = resident.UserId ?? resident.Id;
                    var daysUntilDue = (bill.DueDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days;
                    var message = daysUntilDue > 0
                        ? $"Fatura {bill.BillNumber ?? bill.Id.ToString()[..8]} vence em {daysUntilDue} dia(s)"
                        : daysUntilDue == 0
                            ? $"Fatura {bill.BillNumber ?? bill.Id.ToString()[..8]} vence hoje!"
                            : $"Fatura {bill.BillNumber ?? bill.Id.ToString()[..8]} está em atraso ({-daysUntilDue} dia(s))";

                    var notification = Notification.Create(
                        bill.CondominiumId, userId, "Lembrete de Pagamento",
                        message, NotificationType.Bill, "Bill", bill.Id);

                    await _notificationRepo.AddAsync(notification);
                    reminderCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar lembrete para fatura {BillId}", bill.Id);
            }
        }

        if (reminderCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Lembretes: {Count} notificações de pagamento geradas", reminderCount);
        }
    }
}
