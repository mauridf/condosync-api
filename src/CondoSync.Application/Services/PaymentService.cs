using CondoSync.Application.Features.Bills.DTOs;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class PaymentService
{
    private readonly IPaymentGateway _gateway;
    private readonly IRepository<Bill> _billRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentGateway gateway,
        IRepository<Bill> billRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<PaymentService> logger)
    {
        _gateway = gateway;
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    private async Task<Bill?> GetBillAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var bills = await _billRepository.FindAsync(b => b.Id == id && b.CondominiumId == tenantId);
        return bills.FirstOrDefault();
    }

    public async Task<PaymentBoletoResponse> GenerateBoletoAsync(Guid billId)
    {
        var bill = await GetBillAsync(billId);
        if (bill == null)
            throw new InvalidOperationException("Fatura não encontrada");

        if (bill.Status != BillStatus.Pending && bill.Status != BillStatus.Overdue)
            throw new InvalidOperationException("Fatura não está pendente");

        var boletoCode = await _gateway.GenerateBoletoAsync(
            bill.TotalAmount, bill.Description,
            bill.DueDate.ToDateTime(TimeOnly.MinValue),
            new() { { "bill_id", bill.Id.ToString() }, { "tenant_id", bill.CondominiumId.ToString() } });

        var boletoUrl = $"https://boleto.condosync.app/{bill.Id:N}";

        bill.SetBoleto(boletoUrl, boletoCode);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Boleto gerado para fatura {BillId}: {Code}", billId, boletoCode);

        return new PaymentBoletoResponse(bill.Id, boletoUrl, boletoCode, bill.DueDate.ToString("yyyy-MM-dd"), bill.TotalAmount);
    }

    public async Task<PaymentPixResponse> GeneratePixAsync(Guid billId)
    {
        var bill = await GetBillAsync(billId);
        if (bill == null)
            throw new InvalidOperationException("Fatura não encontrada");

        if (bill.Status != BillStatus.Pending && bill.Status != BillStatus.Overdue)
            throw new InvalidOperationException("Fatura não está pendente");

        var pixCode = await _gateway.GeneratePixAsync(
            bill.TotalAmount, bill.Description,
            new() { { "bill_id", bill.Id.ToString() }, { "tenant_id", bill.CondominiumId.ToString() } });

        var pixQrCodeUrl = $"https://pix.condosync.app/qr/{bill.Id:N}.png";

        bill.SetPix(pixCode, pixQrCodeUrl);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("PIX gerado para fatura {BillId}", billId);

        return new PaymentPixResponse(bill.Id, pixCode, pixQrCodeUrl, bill.TotalAmount);
    }

    public async Task<bool> ProcessPaymentAsync(Guid billId, decimal amount, PaymentMethod method, Guid paidBy)
    {
        var bill = await GetBillAsync(billId);
        if (bill == null)
            throw new InvalidOperationException("Fatura não encontrada");

        var transactionId = $"TX-{billId:N}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var success = await _gateway.ProcessPaymentAsync(transactionId, method);

        if (!success)
        {
            _logger.LogWarning("Pagamento rejeitado pelo gateway para fatura {BillId}", billId);
            return false;
        }

        bill.Pay(amount, method.ToString(), transactionId, paidBy);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Pagamento processado: {TransactionId} para fatura {BillId}", transactionId, billId);

        return true;
    }
}
