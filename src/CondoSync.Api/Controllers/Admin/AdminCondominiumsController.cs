using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Admin.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers.Admin;

[Authorize(AuthenticationSchemes = "Admin", Roles = "super_admin,support,analyst")]
public class AdminCondominiumsController : AdminBaseController
{
    private readonly AdminService _adminService;
    private readonly ILogger<AdminCondominiumsController> _logger;

    public AdminCondominiumsController(
        AdminService adminService,
        ILogger<AdminCondominiumsController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os condomínios com filtros e paginação
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? plan = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var condominiums = await _adminService.GetAllCondominiumsAsync(search, status, plan, page, perPage);

        var response = condominiums.Select(c => new CondominiumListResponse(
            c.Id,
            c.Name,
            c.Slug,
            c.Email,
            c.SubscriptionPlan.ToString(),
            c.SubscriptionStatus.ToString(),
            c.IsActive,
            c.CreatedAt
        ));

        return Ok(new { success = true, data = response, meta = new { page, perPage, total = response.Count() } });
    }

    /// <summary>
    /// Obtém detalhes de um condomínio específico
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var condominium = await _adminService.GetCondominiumByIdAsync(id);

        if (condominium == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        var response = new CondominiumDetailResponse(
            condominium.Id,
            condominium.Name,
            condominium.Cnpj,
            condominium.Slug,
            condominium.Address,
            condominium.City,
            condominium.State,
            condominium.ZipCode,
            condominium.Phone,
            condominium.Email,
            condominium.LogoUrl,
            condominium.SubscriptionPlan.ToString(),
            condominium.SubscriptionStatus.ToString(),
            condominium.SubscriptionExpiresAt,
            condominium.TrialEndsAt,
            condominium.MaxUnits,
            condominium.MaxResidentsPerUnit,
            condominium.Timezone,
            condominium.Language,
            condominium.EnabledModules,
            condominium.IsActive,
            condominium.CreatedAt,
            condominium.UpdatedAt
        );

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Cria um novo condomínio manualmente (SuperAdmin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCondominiumRequest request)
    {
        try
        {
            var condominium = await _adminService.CreateCondominiumAsync(
                request.Name,
                request.Slug,
                request.AdminName,
                request.AdminEmail,
                request.AdminPassword,
                request.Cnpj,
                request.Phone,
                request.Plan);

            return CreatedAtAction(nameof(GetById), new { id = condominium.Id }, new
            {
                success = true,
                data = new
                {
                    condominium.Id,
                    condominium.Name,
                    condominium.Slug,
                    condominium.Email
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
        }
    }

    /// <summary>
    /// Atualiza dados de um condomínio
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCondominiumRequest request)
    {
        var condominium = await _adminService.UpdateCondominiumAsync(
            id,
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.State,
            request.ZipCode);

        if (condominium == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, data = new { condominium.Id, condominium.Name, condominium.UpdatedAt } });
    }

    /// <summary>
    /// Suspende um condomínio
    /// </summary>
    [HttpPatch("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await _adminService.SuspendCondominiumAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, message = "Condomínio suspenso com sucesso" });
    }

    /// <summary>
    /// Ativa um condomínio
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _adminService.ActivateCondominiumAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, message = "Condomínio ativado com sucesso" });
    }

    /// <summary>
    /// Altera o plano de um condomínio
    /// </summary>
    [HttpPatch("{id:guid}/change-plan")]
    public async Task<IActionResult> ChangePlan(Guid id, [FromBody] ChangePlanRequest request)
    {
        var result = await _adminService.ChangePlanAsync(id, request.Plan);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, message = $"Plano alterado para {request.Plan}" });
    }

    /// <summary>
    /// Remove um condomínio (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _adminService.DeleteCondominiumAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, message = "Condomínio removido com sucesso" });
    }

    /// <summary>
    /// Obtém métricas de uso de um condomínio
    /// </summary>
    [HttpGet("{id:guid}/usage")]
    public async Task<IActionResult> GetUsage(Guid id)
    {
        var usage = await _adminService.GetCondominiumUsageAsync(id);

        if (usage == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Condomínio não encontrado" } });

        return Ok(new { success = true, data = usage });
    }
}