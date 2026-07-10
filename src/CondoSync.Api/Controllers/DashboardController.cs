using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class DashboardController : BaseController
{
    private readonly CondoDashboardService _dashboardService;

    public DashboardController(CondoDashboardService dashboardService) => _dashboardService = dashboardService;

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
}