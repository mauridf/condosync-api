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
    private readonly ILogger<BillsController> _logger;

    public BillsController(BillService billService, ILogger<BillsController> logger)
    {
        _billService = billService;
        _logger = logger;
    }

    /// <summary>
    /// Lista faturas com filtros
    /// </summary>
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
            b.PaymentDate?.ToDateTime(TimeOnly.MinValue), b.PaymentAmount, b.CreatedAt
        ));

        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    /// <summary>
    /// Obtém detalhes de uma fatura
    /// </summary>
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
            bill.CreatedAt, bill.UpdatedAt
        );

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Gera uma fatura manualmente
    /// </summary>
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

    /// <summary>
    /// Gera faturas em lote
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> BatchGenerate([FromBody] BatchGenerateBillsRequest request)
    {
        var bills = await _billService.BatchGenerateBillsAsync(
            request.ReferenceMonth, request.Description,
            DateOnly.FromDateTime(request.DueDate), request.UnitIds);

        return Ok(new
        {
            success = true,
            data = new { generatedCount = bills.Count, referenceMonth = request.ReferenceMonth }
        });
    }

    /// <summary>
    /// Registra pagamento de fatura
    /// </summary>
    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayBillRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var bill = await _billService.PayBillAsync(id, request.Amount, request.PaymentMethod, request.TransactionId, userId.Value);

        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });

        return Ok(new { success = true, message = "Pagamento registrado com sucesso" });
    }

    /// <summary>
    /// Cancela uma fatura
    /// </summary>
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var bill = await _billService.CancelBillAsync(id);

        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });

        return Ok(new { success = true, message = "Fatura cancelada com sucesso" });
    }

    /// <summary>
    /// Perdoa uma fatura
    /// </summary>
    [HttpPatch("{id:guid}/waive")]
    public async Task<IActionResult> Waive(Guid id)
    {
        var bill = await _billService.WaiveBillAsync(id);

        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });

        return Ok(new { success = true, message = "Fatura perdoada com sucesso" });
    }

    /// <summary>
    /// Calcula multa e juros de uma fatura
    /// </summary>
    [HttpPost("{id:guid}/calculate-fine")]
    public async Task<IActionResult> CalculateFine(Guid id)
    {
        var bill = await _billService.CalculateFineAsync(id);

        if (bill == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Fatura não encontrada" } });

        return Ok(new
        {
            success = true,
            data = new { bill.Id, bill.TotalAmount, bill.FineAmount, bill.InterestAmount }
        });
    }

    /// <summary>
    /// Lista faturas em atraso
    /// </summary>
    [HttpGet("reports/overdue")]
    public async Task<IActionResult> GetOverdue([FromQuery] Guid? unitId = null)
    {
        var bills = await _billService.GetOverdueBillsAsync(unitId);

        var response = bills.Select(b => new OverdueReportResponse(
            b.Id, b.UnitId, "", b.BillNumber, b.Description, b.ReferenceMonth,
            b.TotalAmount, b.DueDate.ToDateTime(TimeOnly.MinValue),
            (DateTime.UtcNow.Date - b.DueDate.ToDateTime(TimeOnly.MinValue).Date).Days,
            b.FineAmount, b.InterestAmount, b.Status.ToString()
        ));

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Resumo financeiro mensal
    /// </summary>
    [HttpGet("reports/summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] string referenceMonth)
    {
        var summary = await _billService.GetMonthlySummaryAsync(referenceMonth);
        return Ok(new { success = true, data = summary });
    }
}