using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Visitors.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class GuestListsController : BaseController
{
    private readonly VisitorService _visitorService;

    public GuestListsController(VisitorService visitorService) => _visitorService = visitorService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? unitId = null, [FromQuery] DateTime? eventDate = null)
    {
        DateOnly? dateOnly = eventDate.HasValue ? DateOnly.FromDateTime(eventDate.Value) : null;
        var lists = await _visitorService.GetGuestListsAsync(unitId, dateOnly);
        var allVisitors = await _visitorService.GetVisitorsAsync(page: 1, perPage: 1000);

        var response = lists.Select(l => new GuestListResponse(
            l.Id, l.Title, l.Description, l.EventDate.ToString("yyyy-MM-dd"),
            l.StartTime, l.EndTime, l.MaxGuests, l.RequiresQrCode,
            l.Status, allVisitors.Count(v => v.GuestListId == l.Id), l.CreatedAt));
        return Ok(new { success = true, data = response });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var list = await _visitorService.GetGuestListByIdAsync(id);
        if (list == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Lista não encontrada" } });

        var visitors = await _visitorService.GetVisitorsAsync(page: 1, perPage: 1000);
        var count = visitors.Count(v => v.GuestListId == id);

        var response = new GuestListResponse(
            list.Id, list.Title, list.Description, list.EventDate.ToString("yyyy-MM-dd"),
            list.StartTime, list.EndTime, list.MaxGuests, list.RequiresQrCode,
            list.Status, count, list.CreatedAt);
        return Ok(new { success = true, data = response });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGuestListRequest request)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var list = await _visitorService.CreateGuestListAsync(
            userId.Value, request.Title, DateOnly.FromDateTime(request.EventDate),
            request.BookingId, request.UnitId, request.Description,
            request.StartTime, request.EndTime, request.MaxGuests, request.RequiresQrCode);
        return CreatedAtAction(nameof(GetById), new { id = list.Id },
            new { success = true, data = new { list.Id, list.Title, list.EventDate } });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGuestListRequest request)
    {
        var list = await _visitorService.UpdateGuestListAsync(
            id, request.Title, request.Description,
            request.StartTime, request.EndTime, request.MaxGuests, request.RequiresQrCode);
        if (list == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Lista não encontrada" } });
        return Ok(new { success = true, message = "Lista atualizada" });
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _visitorService.CancelGuestListAsync(id);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Lista não encontrada" } });
        return Ok(new { success = true, message = "Lista cancelada" });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _visitorService.DeleteGuestListAsync(id);
        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Lista não encontrada" } });
        return Ok(new { success = true, message = "Lista removida" });
    }
}
