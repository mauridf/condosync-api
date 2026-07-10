using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.CommonAreas.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class CommonAreasController : BaseController
{
    private readonly CommonAreaService _service;

    public CommonAreasController(CommonAreaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? type = null, [FromQuery] bool? isActive = null)
    {
        var areas = await _service.GetAllAsync(type, isActive);
        return Ok(new { success = true, data = areas });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var area = await _service.GetByIdAsync(id);
        return area == null ? NotFound() : Ok(new { success = true, data = area });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCommonAreaRequest request)
    {
        var area = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = area.Id }, new { success = true, data = area });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommonAreaRequest request)
    {
        var area = await _service.UpdateAsync(id, request);
        return area == null ? NotFound() : Ok(new { success = true, data = area });
    }

    [HttpPatch("{id:guid}/maintenance")]
    public async Task<IActionResult> SetMaintenance(Guid id, [FromBody] string description)
    {
        var result = await _service.SetMaintenanceAsync(id, description);
        return result ? Ok(new { success = true }) : NotFound();
    }

    [HttpPatch("{id:guid}/maintenance/complete")]
    public async Task<IActionResult> CompleteMaintenance(Guid id)
    {
        var result = await _service.CompleteMaintenanceAsync(id);
        return result ? Ok(new { success = true }) : NotFound();
    }

    [HttpPatch("{id:guid}/hours")]
    public async Task<IActionResult> SetOperatingHours(Guid id, [FromBody] string operatingHours)
    {
        var result = await _service.SetOperatingHoursAsync(id, operatingHours);
        return result ? Ok(new { success = true }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? Ok(new { success = true }) : NotFound();
    }
}
