using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Employees.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class EmployeesController : BaseController
{
    private readonly EmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(EmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "RequireSubAdminOrAbove", AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _employeeService.GetEmployeesAsync();
        return Ok(new { success = true, data = employees });
    }

    [HttpPost]
    [Authorize(Policy = "RequireSyndic", AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        try
        {
            var employee = await _employeeService.CreateEmployeeAsync(request);
            return CreatedAtAction(nameof(GetAll), new { success = true, data = employee });
        }
        catch (InvalidOperationException ex)
        {
            var code = ex.Message;
            var message = code switch
            {
                "EMAIL_ALREADY_EXISTS" => "Este email já está cadastrado neste condomínio",
                "INVALID_ROLE" => "Cargo inválido para funcionário",
                "UNIT_NOT_FOUND" => "Unidade não encontrada",
                "NO_UNITS_IN_CONDOMINIUM" => "Condomínio sem unidades cadastradas",
                _ => ex.Message
            };
            return BadRequest(new { success = false, error = new { code, message } });
        }
    }
}
