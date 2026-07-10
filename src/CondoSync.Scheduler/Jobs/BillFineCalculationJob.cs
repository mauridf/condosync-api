using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CondoSync.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class BillFineCalculationJob : IJob
{
    private readonly IRepository<Bill> _billRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BillFineCalculationJob> _logger;

    public BillFineCalculationJob(
        IRepository<Bill> billRepo,
        IUnitOfWork unitOfWork,
        ILogger<BillFineCalculationJob> logger)
    {
        _billRepo = billRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateTime.UtcNow;
        var bills = await _billRepo.FindAsync(b =>
            b.Status == BillStatus.Pending ||
            b.Status == BillStatus.Overdue ||
            b.Status == BillStatus.PartiallyPaid);

        var calculatedCount = 0;

        foreach (var bill in bills)
        {
            try
            {
                var oldFine = bill.FineAmount;
                var oldInterest = bill.InterestAmount;

                bill.CalculateFine(today);

                if (bill.FineAmount != oldFine || bill.InterestAmount != oldInterest)
                    calculatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao calcular multa da fatura {BillId}", bill.Id);
            }
        }

        if (calculatedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Multas calculadas: {Count} faturas atualizadas", calculatedCount);
        }
    }
}
