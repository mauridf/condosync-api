using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Bills.DTOs;
using CondoSync.Application.Services;
using CondoSync.Core.Enums;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class BillsController : BaseController
{
    private readonly BillService _billService;
    private readonly PaymentService _paymentService;
    private readonly ILogger<BillsController> _logger;

    public BillsController(BillService billService, PaymentService paymentService, ILogger<BillsController> logger)
    {
        _billService = billService;
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? unitId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? referenceMonth = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var bills = await _billService.GetBillsAsync(unitId, status, referenceMonth, page, perPage);
        var response = bills.Select(b => new BillResponse(
            b.Id, b.UnitId, "", b.BillNumber, b.Description, b.ReferenceMonth,
            b.BaseAmount, b.TotalAmount, b.FineAmount, b.InterestAmount,
            b.DueDate.ToDateTime(TimeOnly.MinValue), b.Status.ToString(),
            b.PaymentDate?.ToDateTime(TimeOnly.MinValue), b.PaymentAmount, b.CreatedAt));
        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var bill = await _billService.GetBillByIdAsync(id);
        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });

        var response = new BillDetailResponse(
            bill.Id, bill.CondominiumId, bill.UnitId, bill.BillNumber,
            bill.Description, bill.ReferenceMonth, bill.BaseAmount,
            bill.DiscountAmount, bill.DiscountType, bill.FineAmount,
            bill.InterestAmount, bill.TotalAmount, bill.Balance,
            bill.IssueDate, bill.DueDate, bill.FineStartDate,
            bill.LateFeePercentage, bill.LateInterestDaily,
            bill.Status.ToString(), bill.PaymentDate, bill.PaymentAmount,
            bill.PaymentMethod, bill.TransactionId,
            bill.InstallmentNumber, bill.TotalInstallments,
            bill.BoletoUrl, bill.BoletoCode, bill.PixCode, bill.PixQrCodeUrl,
            bill.CreatedAt, bill.UpdatedAt);
        return Ok(new { success = true, data = response });
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] GenerateBillRequest request)
    {
        try
        {
            var bill = await _billService.GenerateBillAsync(
                request.UnitId, request.Description, request.ReferenceMonth,
                request.BaseAmount, DateOnly.FromDateTime(request.DueDate),
                request.LateFeePercentage, request.LateInterestDaily);
            return CreatedAtAction(nameof(GetById), new { id = bill.Id }, new
            {
                success = true,
                data = new { bill.Id, bill.BillNumber, bill.TotalAmount }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    [HttpPost("batch")]
    public async Task<IActionResult> BatchGenerate([FromBody] BatchGenerateBillsRequest request)
    {
        var bills = await _billService.BatchGenerateBillsAsync(
            request.ReferenceMonth, request.Description,
            DateOnly.FromDateTime(request.DueDate), request.UnitIds);
        return Ok(new { success = true, data = new { generatedCount = bills.Count, referenceMonth = request.ReferenceMonth } });
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayBillRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var bill = await _billService.PayBillAsync(id, request.Amount, request.PaymentMethod, request.TransactionId, userId.Value);
        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });
        return Ok(new { success = true, message = "Pagamento registrado" });
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var bill = await _billService.CancelBillAsync(id);
        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });
        return Ok(new { success = true, message = "Fatura cancelada" });
    }

    [HttpPatch("{id:guid}/waive")]
    public async Task<IActionResult> Waive(Guid id)
    {
        var bill = await _billService.WaiveBillAsync(id);
        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });
        return Ok(new { success = true, message = "Fatura perdoada" });
    }

    [HttpPost("{id:guid}/calculate-fine")]
    public async Task<IActionResult> CalculateFine(Guid id)
    {
        var bill = await _billService.CalculateFineAsync(id);
        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });
        return Ok(new { success = true, data = new { bill.Id, bill.TotalAmount, bill.FineAmount, bill.InterestAmount } });
    }

    [HttpGet("reports/overdue")]
    public async Task<IActionResult> GetOverdue([FromQuery] Guid? unitId = null)
    {
        var bills = await _billService.GetOverdueBillsAsync(unitId);
        var response = bills.Select(b => new OverdueReportResponse(
            b.Id, b.UnitId, "", b.BillNumber, b.Description, b.ReferenceMonth,
            b.TotalAmount, b.DueDate.ToDateTime(TimeOnly.MinValue),
            (DateTime.UtcNow.Date - b.DueDate.ToDateTime(TimeOnly.MinValue).Date).Days,
            b.FineAmount, b.InterestAmount, b.Status.ToString()));
        return Ok(new { success = true, data = response });
    }

    [HttpGet("reports/summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] string referenceMonth)
    {
        var summary = await _billService.GetMonthlySummaryAsync(referenceMonth);
        return Ok(new { success = true, data = summary });
    }

    // ─── Payment Gateway ─────────────────────────────────────────

    [HttpPost("{id:guid}/boleto")]
    public async Task<IActionResult> GenerateBoleto(Guid id)
    {
        try
        {
            var result = await _paymentService.GenerateBoletoAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    [HttpPost("{id:guid}/pix")]
    public async Task<IActionResult> GeneratePix(Guid id)
    {
        try
        {
            var result = await _paymentService.GeneratePixAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    [HttpPost("{id:guid}/process-payment")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        try
        {
            var method = Enum.Parse<PaymentMethod>(request.PaymentMethod, true);
            var success = await _paymentService.ProcessPaymentAsync(id, request.Amount, method, userId.Value);
            if (!success)
                return BadRequest(new { success = false, error = new { code = "PAYMENT_FAILED", message = "Pagamento rejeitado pela operadora" } });
            return Ok(new { success = true, message = "Pagamento processado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }
}
