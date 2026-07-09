// Arquivo: src/CondoSync.Api/Controllers/ResidentsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Residents.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class ResidentsController : BaseController
{
    private readonly ResidentService _residentService;
    private readonly ILogger<ResidentsController> _logger;

    public ResidentsController(ResidentService residentService, ILogger<ResidentsController> logger)
    {
        _residentService = residentService;
        _logger = logger;
    }

    /// <summary>
    /// Lista moradores do condomínio
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? unitId = null,
        [FromQuery] string? residentType = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var residents = await _residentService.GetResidentsAsync(unitId, residentType, search, page, perPage);

        var response = residents.Select(r => new ResidentResponse(
            r.Id,
            r.UnitId,
            r.UserId?.ToString(),
            r.Name,
            r.ResidentType.ToString(),
            r.Email,
            r.Phone,
            r.Cpf,
            r.IsPrimary,
            r.IsActive,
            r.HasSystemAccess,
            r.MoveInDate,
            r.CreatedAt
        ));

        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    /// <summary>
    /// Obtém detalhes de um morador
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);

        if (resident == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });

        // Deserializar veículos e pets
        var vehicles = string.IsNullOrEmpty(resident.Vehicles) ? null :
            System.Text.Json.JsonSerializer.Deserialize<List<VehicleDTO>>(resident.Vehicles);

        var pets = string.IsNullOrEmpty(resident.Pets) ? null :
            System.Text.Json.JsonSerializer.Deserialize<List<PetDTO>>(resident.Pets);

        var response = new ResidentDetailResponse(
            resident.Id,
            resident.CondominiumId,
            resident.UnitId,
            resident.UserId,
            resident.Name,
            resident.ResidentType.ToString(),
            resident.Email,
            resident.Phone,
            resident.Cpf,
            resident.Rg,
            resident.BirthDate,
            resident.Profession,
            resident.IsPrimary,
            resident.IsActive,
            resident.IsEmergencyContact,
            resident.HasSystemAccess,
            resident.OwnerName,
            resident.OwnerPhone,
            resident.OwnerEmail,
            resident.MoveInDate,
            resident.MoveOutDate,
            vehicles,
            pets,
            resident.CreatedAt,
            resident.UpdatedAt
        );

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Cria um novo morador
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResidentRequest request)
    {
        try
        {
            var resident = await _residentService.CreateResidentAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = resident.Id }, new
            {
                success = true,
                data = new { resident.Id, resident.Name, resident.ResidentType, resident.UnitId }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, error = new { code = "VALIDATION_ERROR", message = ex.Message } });
        }
    }

    /// <summary>
    /// Atualiza um morador
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResidentRequest request)
    {
        var resident = await _residentService.UpdateResidentAsync(id, request);

        if (resident == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });

        return Ok(new { success = true, data = new { resident.Id, resident.Name, resident.UpdatedAt } });
    }

    /// <summary>
    /// Remove um morador (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _residentService.DeleteResidentAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });

        return Ok(new { success = true, message = "Morador removido com sucesso" });
    }

    /// <summary>
    /// Lista moradores de uma unidade específica
    /// </summary>
    [HttpGet("by-unit/{unitId:guid}")]
    public async Task<IActionResult> GetByUnit(Guid unitId)
    {
        var residents = await _residentService.GetResidentsByUnitAsync(unitId);

        var response = residents.Select(r => new ResidentResponse(
            r.Id,
            r.UnitId,
            r.UserId?.ToString(),
            r.Name,
            r.ResidentType.ToString(),
            r.Email,
            r.Phone,
            r.Cpf,
            r.IsPrimary,
            r.IsActive,
            r.HasSystemAccess,
            r.MoveInDate,
            r.CreatedAt
        ));

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Libera ou bloqueia acesso ao sistema para um morador
    /// </summary>
    [HttpPost("{id:guid}/toggle-access")]
    public async Task<IActionResult> ToggleAccess(Guid id, [FromBody] ToggleAccessRequest request)
    {
        var result = await _residentService.ToggleAccessAsync(id, request.GrantAccess);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });

        var message = request.GrantAccess ? "Acesso liberado com sucesso" : "Acesso bloqueado com sucesso";
        return Ok(new { success = true, message });
    }
}