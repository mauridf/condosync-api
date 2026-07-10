using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class DashboardController : BaseController
{
    private readonly CondoDashboardService _dashboardService;
    private readonly ResidentDashboardService _residentDashboardService;

    public DashboardController(CondoDashboardService dashboardService, ResidentDashboardService residentDashboardService)
    {
        _dashboardService = dashboardService;
        _residentDashboardService = residentDashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(new { success = true, data = summary });
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int count = 10)
    {
        var activity = await _dashboardService.GetRecentActivityAsync(count);
        return Ok(new { success = true, data = activity });
    }

    [HttpGet("advanced")]
    public async Task<IActionResult> GetAdvancedStats()
    {
        var stats = await _dashboardService.GetAdvancedStatsAsync();
        return Ok(new { success = true, data = stats });
    }

    [HttpGet("resident")]
    public async Task<IActionResult> GetResidentDashboard()
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var dashboard = await _residentDashboardService.GetResidentDashboardAsync(userId.Value);
            return Ok(new { success = true, data = dashboard });
        }
        catch (InvalidOperationException ex) when (ex.Message == "RESIDENT_NOT_FOUND")
        {
            return NotFound(new { success = false, error = new { code = "RESIDENT_NOT_FOUND", message = "Perfil de morador não encontrado para este usuário" } });
        }
    }
}