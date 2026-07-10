using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Notifications.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class NotificationTemplatesController : BaseController
{
    private readonly NotificationTemplateService _templateService;

    public NotificationTemplatesController(NotificationTemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? notificationType = null)
    {
        var templates = await _templateService.GetAllAsync(notificationType);
        return Ok(new { success = true, data = templates });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var template = await _templateService.GetByIdAsync(id);
        return template == null ? NotFound() : Ok(new { success = true, data = template });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationTemplateRequest request)
    {
        var template = await _templateService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = template.Id }, new { success = true, data = template });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNotificationTemplateRequest request)
    {
        var template = await _templateService.UpdateAsync(id, request);
        return template == null ? NotFound() : Ok(new { success = true, data = template });
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var result = await _templateService.ToggleActiveAsync(id);
        return result ? Ok(new { success = true }) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _templateService.DeleteAsync(id);
        return result ? Ok(new { success = true }) : NotFound();
    }
}
