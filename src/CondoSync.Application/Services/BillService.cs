using CondoSync.Application.Features.Bills.DTOs;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class BillService
{
    private readonly IRepository<Bill> _billRepository;
    private readonly IRepository<Unit> _unitRepository;
    private readonly IRepository<CondominiumSettings> _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<BillService> _logger;

    public BillService(
        IRepository<Bill> billRepository,
        IRepository<Unit> unitRepository,
        IRepository<CondominiumSettings> settingsRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<BillService> logger)
    {
        _billRepository = billRepository;
        _unitRepository = unitRepository;
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Bill>> GetBillsAsync(
        Guid? unitId = null,
        string? status = null,
        string? referenceMonth = null,
        int page = 1,
        int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();

        var bills = await _billRepository.FindAsync(b => b.CondominiumId == tenantId);

        var query = bills.AsQueryable();

        if (unitId.HasValue)
            query = query.Where(b => b.UnitId == unitId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var billStatus = Enum.Parse<BillStatus>(status, true);
            query = query.Where(b => b.Status == billStatus);
        }

        if (!string.IsNullOrWhiteSpace(referenceMonth))
            query = query.Where(b => b.ReferenceMonth == referenceMonth);

        return query
            .OrderByDescending(b => b.DueDate)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Bill?> GetBillByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var bills = await _billRepository.FindAsync(b => b.Id == id && b.CondominiumId == tenantId);
        return bills.FirstOrDefault();
    }

    public async Task<Bill> GenerateBillAsync(
        Guid unitId,
        string description,
        string referenceMonth,
        decimal baseAmount,
        DateOnly dueDate,
        decimal? lateFeePercentage = null,
        decimal? lateInterestDaily = null)
    {
        var tenantId = GetCurrentTenantId();

        // Verificar se unidade existe
        var units = await _unitRepository.FindAsync(u => u.Id == unitId && u.CondominiumId == tenantId);
        var unit = units.FirstOrDefault();

        if (unit == null)
            throw new InvalidOperationException("Unidade não encontrada");

        // Buscar configurações padrão
        var settingsList = await _settingsRepository.FindAsync(s => s.CondominiumId == tenantId);
        var settings = settingsList.FirstOrDefault();

        var feePercentage = lateFeePercentage ?? settings?.LateFeePercentage ?? 2.00m;
        var interestDaily = lateInterestDaily ?? settings?.LateInterestDaily ?? 0.033m;

        var bill = Bill.Create(
            tenantId,
            unitId,
            description,
            referenceMonth,
            baseAmount,
            dueDate,
            feePercentage,
            interestDaily);

        await _billRepository.AddAsync(bill);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Fatura gerada: {BillId} para unidade {UnitId} - {Amount:C}",
            bill.Id, unitId, baseAmount);

        return bill;
    }

    public async Task<List<Bill>> BatchGenerateBillsAsync(
        string referenceMonth,
        string description,
        DateOnly dueDate,
        List<Guid>? unitIds = null)
    {
        var tenantId = GetCurrentTenantId();
        var generatedBills = new List<Bill>();

        // Buscar unidades (todas ou filtradas)
        var units = await _unitRepository.FindAsync(u =>
            u.CondominiumId == tenantId && u.IsActive);

        if (unitIds != null && unitIds.Any())
            units = units.Where(u => unitIds.Contains(u.Id));

        foreach (var unit in units)
        {
            // Verificar se já existe fatura para este mês
            var existingBills = await _billRepository.FindAsync(b =>
                b.UnitId == unit.Id && b.ReferenceMonth == referenceMonth);

            if (existingBills.Any()) continue;

            var monthlyFee = unit.MonthlyFee ?? 0;
            if (monthlyFee <= 0) continue;

            var bill = Bill.Create(
                tenantId,
                unit.Id,
                description,
                referenceMonth,
                monthlyFee,
                dueDate,
                unit.LateFeePercentage,
                unit.InterestPercentage);

            await _billRepository.AddAsync(bill);
            generatedBills.Add(bill);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{Count} faturas geradas em lote para {Month}",
            generatedBills.Count, referenceMonth);

        return generatedBills;
    }

    public async Task<Bill?> PayBillAsync(Guid id, decimal amount, string paymentMethod, string transactionId, Guid paidBy)
    {
        var bill = await GetBillByIdAsync(id);
        if (bill == null) return null;

        bill.Pay(amount, paymentMethod, transactionId, paidBy);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Fatura {BillId} paga: {Amount:C}", id, amount);

        return bill;
    }

    public async Task<Bill?> CalculateFineAsync(Guid id)
    {
        var bill = await GetBillByIdAsync(id);
        if (bill == null) return null;

        bill.CalculateFine(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();

        return bill;
    }

    public async Task<Bill?> CancelBillAsync(Guid id)
    {
        var bill = await GetBillByIdAsync(id);
        if (bill == null) return null;

        bill.Cancel("Cancelado manualmente");
        await _unitOfWork.SaveChangesAsync();

        return bill;
    }

    public async Task<Bill?> WaiveBillAsync(Guid id)
    {
        var bill = await GetBillByIdAsync(id);
        if (bill == null) return null;

        bill.Waive();
        await _unitOfWork.SaveChangesAsync();

        return bill;
    }

    public async Task<List<Bill>> GetOverdueBillsAsync(Guid? unitId = null)
    {
        var tenantId = GetCurrentTenantId();

        var bills = await _billRepository.FindAsync(b =>
            b.CondominiumId == tenantId &&
            (b.Status == BillStatus.Overdue || b.Status == BillStatus.PartiallyPaid));

        if (unitId.HasValue)
            bills = bills.Where(b => b.UnitId == unitId.Value);

        return bills.OrderByDescending(b => b.DueDate).ToList();
    }

    public async Task<BillSummaryResponse> GetMonthlySummaryAsync(string referenceMonth)
    {
        var tenantId = GetCurrentTenantId();

        var bills = await _billRepository.FindAsync(b =>
            b.CondominiumId == tenantId && b.ReferenceMonth == referenceMonth);

        var billsList = bills.ToList();

        return new BillSummaryResponse(
            referenceMonth,
            billsList.Count,
            billsList.Count(b => b.Status == BillStatus.Paid),
            billsList.Count(b => b.Status == BillStatus.Pending),
            billsList.Count(b => b.Status == BillStatus.Overdue),
            billsList.Sum(b => b.TotalAmount),
            billsList.Where(b => b.Status == BillStatus.Paid).Sum(b => b.PaymentAmount ?? 0),
            billsList.Where(b => b.Status == BillStatus.Pending).Sum(b => b.TotalAmount)
        );
    }
}