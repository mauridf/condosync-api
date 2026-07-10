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
            r.Id, r.UnitId, r.UserId?.ToString(), r.Name, r.ResidentType.ToString(),
            r.Email, r.Phone, r.Cpf, r.IsPrimary, r.IsActive, r.HasSystemAccess, r.MoveInDate, r.CreatedAt));
        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var resident = await _residentService.GetResidentByIdAsync(id);
        if (resident == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });

        var vehicles = resident.GetVehicles().Select(v => new VehicleResponse(v.Id, v.Plate, v.Model, v.Color, v.Brand)).ToList();
        var pets = resident.GetPets().Select(p => new PetResponse(p.Id, p.Name, p.Species, p.Breed, p.Color)).ToList();

        var response = new ResidentDetailResponse(
            resident.Id, resident.CondominiumId, resident.UnitId, resident.UserId,
            resident.Name, resident.ResidentType.ToString(), resident.Email, resident.Phone,
            resident.Cpf, resident.Rg, resident.BirthDate, resident.Profession,
            resident.IsPrimary, resident.IsActive, resident.IsEmergencyContact, resident.HasSystemAccess,
            resident.OwnerName, resident.OwnerPhone, resident.OwnerEmail,
            resident.MoveInDate, resident.MoveOutDate, vehicles, pets,
            resident.CreatedAt, resident.UpdatedAt);

        return Ok(new { success = true, data = response });
    }

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateResidentRequest request)
    {
        var resident = await _residentService.UpdateResidentAsync(id, request);
        if (resident == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });
        return Ok(new { success = true, data = new { resident.Id, resident.Name, resident.UpdatedAt } });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _residentService.DeleteResidentAsync(id);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });
        return Ok(new { success = true, message = "Morador removido com sucesso" });
    }

    [HttpGet("by-unit/{unitId:guid}")]
    public async Task<IActionResult> GetByUnit(Guid unitId)
    {
        var residents = await _residentService.GetResidentsByUnitAsync(unitId);
        var response = residents.Select(r => new ResidentResponse(
            r.Id, r.UnitId, r.UserId?.ToString(), r.Name, r.ResidentType.ToString(),
            r.Email, r.Phone, r.Cpf, r.IsPrimary, r.IsActive, r.HasSystemAccess, r.MoveInDate, r.CreatedAt));
        return Ok(new { success = true, data = response });
    }

    [HttpPost("{id:guid}/toggle-access")]
    public async Task<IActionResult> ToggleAccess(Guid id, [FromBody] ToggleAccessRequest request)
    {
        var result = await _residentService.ToggleAccessAsync(id, request.GrantAccess);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });
        var message = request.GrantAccess ? "Acesso liberado" : "Acesso bloqueado";
        return Ok(new { success = true, message });
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "RequireSyndic")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateResidentRoleRequest request)
    {
        try
        {
            var (resident, oldRole, newRole) = await _residentService.UpdateResidentRoleAsync(id, request.Role);
            return Ok(new { success = true, data = new { resident.Id, oldRole, newRole } });
        }
        catch (InvalidOperationException ex)
        {
            var code = ex.Message;
            var message = code switch
            {
                "RESIDENT_NOT_FOUND" => "Morador não encontrado",
                "INVALID_ROLE" => "Cargo inválido. Valores válidos: CondoAdmin, SubAdmin, Employee, Owner, Tenant, Resident",
                "ROLE_NOT_ALLOWED" => "Este cargo não pode ser atribuído manualmente",
                _ => ex.Message
            };
            return BadRequest(new { success = false, error = new { code, message } });
        }
    }

    // ─── Veículos ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/vehicles")]
    public async Task<IActionResult> AddVehicle(Guid id, [FromBody] AddVehicleRequest request)
    {
        var vehicle = await _residentService.AddVehicleAsync(id, request.Plate, request.Model, request.Color, request.Brand);
        if (vehicle == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });
        return CreatedAtAction(nameof(GetById), new { id }, new { success = true, data = vehicle });
    }

    [HttpDelete("{id:guid}/vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> RemoveVehicle(Guid id, Guid vehicleId)
    {
        var result = await _residentService.RemoveVehicleAsync(id, vehicleId);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Veículo não encontrado" } });
        return Ok(new { success = true, message = "Veículo removido" });
    }

    [HttpPut("{id:guid}/vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid id, Guid vehicleId, [FromBody] UpdateVehicleRequest request)
    {
        var result = await _residentService.UpdateVehicleAsync(id, vehicleId, request.Plate, request.Model, request.Color, request.Brand);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Veículo não encontrado" } });
        return Ok(new { success = true, message = "Veículo atualizado" });
    }

    // ─── Pets ──────────────────────────────────────────────────────

    [HttpPost("{id:guid}/pets")]
    public async Task<IActionResult> AddPet(Guid id, [FromBody] AddPetRequest request)
    {
        var pet = await _residentService.AddPetAsync(id, request.Name, request.Species, request.Breed, request.Color);
        if (pet == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Morador não encontrado" } });
        return CreatedAtAction(nameof(GetById), new { id }, new { success = true, data = pet });
    }

    [HttpDelete("{id:guid}/pets/{petId:guid}")]
    public async Task<IActionResult> RemovePet(Guid id, Guid petId)
    {
        var result = await _residentService.RemovePetAsync(id, petId);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Pet não encontrado" } });
        return Ok(new { success = true, message = "Pet removido" });
    }

    [HttpPut("{id:guid}/pets/{petId:guid}")]
    public async Task<IActionResult> UpdatePet(Guid id, Guid petId, [FromBody] UpdatePetRequest request)
    {
        var result = await _residentService.UpdatePetAsync(id, petId, request.Name, request.Species, request.Breed, request.Color);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Pet não encontrado" } });
        return Ok(new { success = true, message = "Pet atualizado" });
    }
}