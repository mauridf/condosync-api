using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondoSync.Application.Features.Units.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[Authorize(AuthenticationSchemes = "Tenant")]
public class UnitsController : BaseController
{
    private readonly UnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(UnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    /// <summary>
    /// Lista unidades do condomínio com filtros
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? block = null,
        [FromQuery] string? floor = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 20)
    {
        var units = await _unitService.GetUnitsAsync(block, floor, status, page, perPage);

        var response = units.Select(u => new UnitResponse(
            u.Id,
            u.Block,
            u.Number,
            u.Floor,
            u.Type.ToString(),
            u.Area,
            u.Bedrooms,
            u.Bathrooms,
            u.ParkingSpots,
            u.OccupancyStatus.ToString(),
            u.IsActive,
            u.MonthlyFee,
            u.CreatedAt
        ));

        return Ok(new { success = true, data = response, meta = new { page, perPage } });
    }

    /// <summary>
    /// Obtém detalhes de uma unidade
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var unit = await _unitService.GetUnitByIdAsync(id);

        if (unit == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Unidade não encontrada" } });

        var response = new UnitDetailResponse(
            unit.Id,
            unit.CondominiumId,
            unit.Block,
            unit.Number,
            unit.Floor,
            unit.Type.ToString(),
            unit.Area,
            unit.Bedrooms,
            unit.Bathrooms,
            unit.ParkingSpots,
            unit.IsActive,
            unit.IsRented,
            unit.OccupancyStatus.ToString(),
            unit.MonthlyFee,
            unit.LateFeePercentage,
            unit.InterestPercentage,
            unit.IptuAnnual,
            unit.CreatedAt,
            unit.UpdatedAt
        );

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Cria uma nova unidade
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request)
    {
        try
        {
            var unit = await _unitService.CreateUnitAsync(
                request.Number,
                request.Type,
                request.Block,
                request.Floor,
                request.Area,
                request.Bedrooms,
                request.Bathrooms,
                request.ParkingSpots,
                request.MonthlyFee);

            return CreatedAtAction(nameof(GetById), new { id = unit.Id }, new
            {
                success = true,
                data = new { unit.Id, unit.Block, unit.Number, unit.Type }
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
        }
    }

    /// <summary>
    /// Cria unidades em lote
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> BatchCreate([FromBody] BatchCreateUnitsRequest request)
    {
        var units = await _unitService.BatchCreateUnitsAsync(request.Units);

        return CreatedAtAction(nameof(GetAll), new
        {
            success = true,
            data = new { created = units.Count, units = units.Select(u => new { u.Id, u.Block, u.Number }) }
        });
    }

    /// <summary>
    /// Atualiza uma unidade
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUnitRequest request)
    {
        var unit = await _unitService.UpdateUnitAsync(
            id,
            request.Number,
            request.Type,
            request.Block,
            request.Floor,
            request.Area,
            request.Bedrooms,
            request.Bathrooms,
            request.ParkingSpots,
            request.MonthlyFee);

        if (unit == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Unidade não encontrada" } });

        return Ok(new { success = true, data = new { unit.Id, unit.Number, unit.UpdatedAt } });
    }

    /// <summary>
    /// Remove uma unidade (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _unitService.DeleteUnitAsync(id);

            if (!result)
                return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Unidade não encontrada" } });

            return Ok(new { success = true, message = "Unidade removida com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
        }
    }

    /// <summary>
    /// Lista blocos do condomínio
    /// </summary>
    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks()
    {
        var blocks = await _unitService.GetBlocksAsync();
        return Ok(new { success = true, data = blocks });
    }
}