using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class EventsController : BaseController
{
    private readonly EventStoreService _eventStoreService;

    public EventsController(EventStoreService eventStoreService) => _eventStoreService = eventStoreService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? aggregateType = null,
        [FromQuery] Guid? aggregateId = null, [FromQuery] string? eventType = null,
        [FromQuery] int page = 1, [FromQuery] int perPage = 50)
    {
        var events = await _eventStoreService.GetEventsAsync(aggregateType, aggregateId, eventType, page, perPage);
        return Ok(new { success = true, data = events, meta = new { page, perPage } });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var evt = await _eventStoreService.GetEventByIdAsync(id);
        if (evt == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Evento não encontrado" } });
        return Ok(new { success = true, data = evt });
    }

    [HttpGet("aggregate/{aggregateId:guid}")]
    public async Task<IActionResult> GetAggregateHistory(Guid aggregateId)
    {
        var events = await _eventStoreService.GetAggregateHistoryAsync(aggregateId);
        return Ok(new { success = true, data = events });
    }
}
