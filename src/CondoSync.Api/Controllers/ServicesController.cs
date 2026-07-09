using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Services.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class ServicesController : BaseController
{
    private readonly ServiceManagementService _serviceManagement;
    private readonly ILogger<ServicesController> _logger;

    public ServicesController(
        ServiceManagementService serviceManagement,
        ILogger<ServicesController> logger)
    {
        _serviceManagement = serviceManagement;
        _logger = logger;
    }

    /// <summary>
    /// Lista serviços do condomínio
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive = null)
    {
        var services = await _serviceManagement.GetServicesAsync(isActive);

        var response = services.Select(s => new ServiceResponse(
            s.Id, s.Name, s.Slug, s.Category, s.ServiceType.ToString(),
            s.Description, s.Icon, s.Price, s.PriceUnit,
            s.RequiresApproval, s.RequiresPayment, s.IsActive,
            s.DisplayOrder, s.CreatedAt
        ));

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Obtém detalhes de um serviço
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var service = await _serviceManagement.GetServiceByIdAsync(id);

        if (service == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Serviço não encontrado" } });

        return Ok(new { success = true, data = service });
    }

    /// <summary>
    /// Cria um novo serviço
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
    {
        try
        {
            var service = await _serviceManagement.CreateServiceAsync(
                request.Name, request.Slug, request.Category, request.ServiceType,
                request.Description, request.Price,
                request.RequiresApproval, request.RequiresPayment);

            return CreatedAtAction(nameof(GetById), new { id = service.Id }, new
            {
                success = true,
                data = new { service.Id, service.Name, service.Slug }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
        }
    }

    /// <summary>
    /// Atualiza um serviço
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceRequest request)
    {
        var service = await _serviceManagement.UpdateServiceAsync(
            id, request.Name, request.Description, request.Price, request.RequiresApproval);

        if (service == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Serviço não encontrado" } });

        return Ok(new { success = true, data = new { service.Id, service.Name, service.UpdatedAt } });
    }

    /// <summary>
    /// Ativa/desativa um serviço
    /// </summary>
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        var result = await _serviceManagement.ToggleServiceAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Serviço não encontrado" } });

        return Ok(new { success = true, message = "Status do serviço alterado com sucesso" });
    }

    /// <summary>
    /// Remove um serviço (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _serviceManagement.DeleteServiceAsync(id);

        if (!result)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Serviço não encontrado" } });

        return Ok(new { success = true, message = "Serviço removido com sucesso" });
    }
}