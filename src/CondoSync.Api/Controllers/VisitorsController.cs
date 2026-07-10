using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Services;
using CondoSync.Core.Enums;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class VisitorsController : BaseController
{
    private readonly VisitorService _visitorService;

    public VisitorsController(VisitorService visitorService)
    {
        _visitorService = visitorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? unitId = null, [FromQuery] DateTime? date = null)
    {
        var visitors = await _visitorService.GetVisitorsAsync(unitId, date);
        return Ok(new { success = true, data = visitors });
    }

    [HttpPost]
    public async Task<IActionResult> Authorize([FromBody] AuthorizeVisitorRequest request)
    {
        var visitorType = Enum.Parse<VisitorType>(request.VisitorType, true);
        var visitor = await _visitorService.AuthorizeVisitorAsync(
            request.UnitId, request.Name, DateOnly.FromDateTime(request.VisitDate),
            visitorType, request.ResidentId, request.Phone,
            request.CompanyName, request.Notes);
        return CreatedAtAction(nameof(GetAll), new { id = visitor.Id }, new { success = true, data = visitor });
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
}

// DTOs inline para simplificar
public record AuthorizeVisitorRequest(
    Guid UnitId, string Name, DateTime VisitDate, string VisitorType = "Guest",
    Guid? ResidentId = null, string? Phone = null, string? CompanyName = null, string? Notes = null
);