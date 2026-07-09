using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Infrastructure.Services;

namespace CondoSync.Api.Controllers.Admin;

[Authorize(AuthenticationSchemes = "Admin", Roles = "super_admin,support,analyst")]
public class AdminDashboardController : AdminBaseController
{
    private readonly AdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(
        AdminDashboardService dashboardService,
        ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Resumo geral do SaaS com métricas principais
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return Ok(new { success = true, data = summary });
    }

    /// <summary>
    /// Distribuição de planos de assinatura
    /// </summary>
    [HttpGet("subscriptions")]
    public async Task<IActionResult> GetSubscriptionDistribution()
    {
        var distribution = await _dashboardService.GetSubscriptionDistributionAsync();
        return Ok(new { success = true, data = distribution });
    }

    /// <summary>
    /// Métricas de crescimento (últimos 12 meses)
    /// </summary>
    [HttpGet("growth")]
    public async Task<IActionResult> GetGrowth()
    {
        var growth = await _dashboardService.GetGrowthMetricsAsync();
        return Ok(new { success = true, data = growth });
    }

    /// <summary>
    /// Taxa de churn (cancelamentos)
    /// </summary>
    [HttpGet("churn")]
    public async Task<IActionResult> GetChurnRate()
    {
        var churn = await _dashboardService.GetChurnRateAsync();
        return Ok(new { success = true, data = churn });
    }

    /// <summary>
    /// Últimos condomínios registrados
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentCondominiums([FromQuery] int count = 10)
    {
        var recent = await _dashboardService.GetRecentCondominiumsAsync(count);
        return Ok(new { success = true, data = recent });
    }

    /// <summary>
    /// Estatísticas gerais do sistema
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        var stats = await _dashboardService.GetSystemStatsAsync();
        return Ok(new { success = true, data = stats });
    }
}