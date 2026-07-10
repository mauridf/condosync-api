using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class ReportsController : BaseController
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService) => _reportService = reportService;

    [HttpGet("bills/pdf")]
    public async Task<IActionResult> GetBillsPdf([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? status)
    {
        var bytes = await _reportService.GenerateBillsPdfAsync(startDate, endDate, status);
        return File(bytes, "application/pdf", $"faturas-{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("bills/excel")]
    public async Task<IActionResult> GetBillsExcel([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? status)
    {
        var bytes = await _reportService.GenerateBillsExcelAsync(startDate, endDate, status);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"faturas-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("bookings/pdf")]
    public async Task<IActionResult> GetBookingsPdf([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? status)
    {
        var bytes = await _reportService.GenerateBookingsPdfAsync(startDate, endDate, status);
        return File(bytes, "application/pdf", $"reservas-{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("bookings/excel")]
    public async Task<IActionResult> GetBookingsExcel([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? status)
    {
        var bytes = await _reportService.GenerateBookingsExcelAsync(startDate, endDate, status);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"reservas-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("residents/pdf")]
    public async Task<IActionResult> GetResidentsPdf()
    {
        var bytes = await _reportService.GenerateResidentsPdfAsync();
        return File(bytes, "application/pdf", $"moradores-{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("residents/excel")]
    public async Task<IActionResult> GetResidentsExcel()
    {
        var bytes = await _reportService.GenerateResidentsExcelAsync();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"moradores-{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("activity/pdf")]
    public async Task<IActionResult> GetActivityPdf([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? action)
    {
        var bytes = await _reportService.GenerateActivityPdfAsync(startDate, endDate, action);
        return File(bytes, "application/pdf", $"atividades-{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("activity/excel")]
    public async Task<IActionResult> GetActivityExcel([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? action)
    {
        var bytes = await _reportService.GenerateActivityExcelAsync(startDate, endDate, action);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"atividades-{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
