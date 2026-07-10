using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Visitors.DTOs;
using CondoSync.Application.Services;
using CondoSync.Core.Enums;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class VisitorsController : BaseController
{
    private readonly VisitorService _visitorService;

    public VisitorsController(VisitorService visitorService) => _visitorService = visitorService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? unitId = null, [FromQuery] DateTime? date = null,
        [FromQuery] string? status = null, [FromQuery] string? search = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 20)
    {
        var visitors = await _visitorService.GetVisitorsAsync(unitId, date, status, search, page, perPage);
        var response = visitors.Select(v => new VisitorResponse(
            v.Id, v.UnitId, v.Name, v.VisitDate.ToString("yyyy-MM-dd"),
            v.VisitorType.ToString(), v.Status.ToString(), v.AuthorizationCode,
            v.Phone, v.CompanyName, v.Document, v.VehiclePlate,
            v.EntryTime, v.ExitTime, v.GuestListId, v.CreatedAt));
        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var v = await _visitorService.GetVisitorByIdAsync(id);
        if (v == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Visitante não encontrado" } });

        var response = new VisitorDetailResponse(
            v.Id, v.CondominiumId, v.UnitId, v.ResidentId,
            v.Name, v.VisitDate.ToString("yyyy-MM-dd"), v.VisitorType.ToString(),
            v.Status.ToString(), v.AuthorizationCode, v.Phone, v.CompanyName,
            v.Document, v.DocumentType, v.VehiclePlate, v.VehicleModel,
            v.ExpectedEntryTime, v.ExpectedExitTime, v.EntryTime, v.ExitTime,
            v.Notes, v.IsRecurring, v.GuestListId, v.CreatedAt, v.UpdatedAt);
        return Ok(new { success = true, data = response });
    }

    [HttpPost]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeVisitorRequest request)
    {
        var visitorType = Enum.Parse<VisitorType>(request.VisitorType, true);
        var visitor = await _visitorService.AuthorizeVisitorAsync(
            request.UnitId, request.Name, DateOnly.FromDateTime(request.VisitDate),
            visitorType, request.ResidentId, request.Phone,
            request.CompanyName, request.Notes, request.Document, request.DocumentType,
            request.VehiclePlate, request.VehicleModel,
            request.ExpectedEntryTime, request.ExpectedExitTime,
            request.GuestListId);
        return CreatedAtAction(nameof(GetById), new { id = visitor.Id },
            new { success = true, data = new { visitor.Id, visitor.Name, visitor.AuthorizationCode } });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVisitorRequest request)
    {
        var visitor = await _visitorService.UpdateVisitorAsync(id, request.Name, request.Phone, request.Notes);
        if (visitor == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Visitante não encontrado" } });
        return Ok(new { success = true, message = "Visitante atualizado" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _visitorService.DeleteVisitorAsync(id);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Visitante não encontrado" } });
        return Ok(new { success = true, message = "Visitante removido" });
    }

    [HttpPatch("{id:guid}/arrive")]
    public async Task<IActionResult> Arrive(Guid id)
    {
        var visitor = await _visitorService.RegisterEntryAsync(id);
        return visitor == null ? NotFound() : Ok(new { success = true, message = "Entrada registrada" });
    }

    [HttpPatch("{id:guid}/depart")]
    public async Task<IActionResult> Depart(Guid id)
    {
        var visitor = await _visitorService.RegisterExitAsync(id);
        return visitor == null ? NotFound() : Ok(new { success = true, message = "Saída registrada" });
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var visitor = await _visitorService.CancelAuthorizationAsync(id);
        return visitor == null ? NotFound() : Ok(new { success = true, message = "Autorização cancelada" });
    }

    [HttpPost("{id:guid}/guest-list/{guestListId:guid}")]
    public async Task<IActionResult> LinkToGuestList(Guid id, Guid guestListId)
    {
        var visitor = await _visitorService.LinkToGuestListAsync(id, guestListId);
        if (visitor == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Visitante ou lista não encontrados" } });
        return Ok(new { success = true, message = "Visitante vinculado à lista de convidados" });
    }
}
