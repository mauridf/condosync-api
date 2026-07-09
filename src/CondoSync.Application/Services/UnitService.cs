using CondoSync.Application.Features.Units.DTOs;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class UnitService
{
    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<Bill> _billRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnitService> _logger;

    public UnitService(
        IRepository<Unit> unitRepo,
        IRepository<Bill> billRepo,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork,
        ILogger<UnitService> logger)
    {
        _unitRepo = unitRepo;
        _billRepo = billRepo;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Unit>> GetUnitsAsync(
        string? block = null,
        string? floor = null,
        string? status = null,
        int page = 1,
        int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var allUnits = (await _unitRepo.GetAllAsync())
            .Where(u => u.CondominiumId == tenantId);

        if (!string.IsNullOrWhiteSpace(block))
            allUnits = allUnits.Where(u => u.Block == block);

        if (!string.IsNullOrWhiteSpace(floor))
            allUnits = allUnits.Where(u => u.Floor == floor);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var occupancyStatus = Enum.Parse<UnitOccupancyStatus>(status, true);
            allUnits = allUnits.Where(u => u.OccupancyStatus == occupancyStatus);
        }

        return allUnits
            .OrderBy(u => u.Block)
            .ThenBy(u => u.Number)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Unit?> GetUnitByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var unit = await _unitRepo.GetByIdAsync(id);
        return unit?.CondominiumId == tenantId ? unit : null;
    }

    public async Task<Unit> CreateUnitAsync(
        string number,
        string type,
        string? block = null,
        string? floor = null,
        decimal? area = null,
        int bedrooms = 0,
        int bathrooms = 0,
        int parkingSpots = 0,
        decimal? monthlyFee = null)
    {
        var tenantId = GetCurrentTenantId();
        var unitType = Enum.Parse<UnitType>(type, true);

        var allUnits = await _unitRepo.GetAllAsync();
        var exists = allUnits.Any(u =>
            u.CondominiumId == tenantId
            && u.Block == block
            && u.Number == number);

        if (exists)
            throw new InvalidOperationException($"Unidade {block}-{number} já existe");

        var unit = Unit.Create(
            tenantId,
            number,
            unitType,
            block: block,
            floor: floor,
            area: area,
            bedrooms: bedrooms,
            bathrooms: bathrooms,
            parkingSpots: parkingSpots,
            monthlyFee: monthlyFee);

        await _unitRepo.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Unidade criada: {Block}-{Number} no tenant {TenantId}",
            block, number, tenantId);

        return unit;
    }

    public async Task<List<Unit>> BatchCreateUnitsAsync(List<CreateUnitRequest> units)
    {
        var tenantId = GetCurrentTenantId();
        var createdUnits = new List<Unit>();

        foreach (var unitRequest in units)
        {
            var unitType = Enum.Parse<UnitType>(unitRequest.Type, true);

            var unit = Unit.Create(
                tenantId,
                unitRequest.Number,
                unitType,
                block: unitRequest.Block,
                floor: unitRequest.Floor,
                area: unitRequest.Area,
                bedrooms: unitRequest.Bedrooms,
                bathrooms: unitRequest.Bathrooms,
                parkingSpots: unitRequest.ParkingSpots,
                monthlyFee: unitRequest.MonthlyFee);

            await _unitRepo.AddAsync(unit);
            createdUnits.Add(unit);
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("{Count} unidades criadas em lote no tenant {TenantId}",
            createdUnits.Count, tenantId);

        return createdUnits;
    }

    public async Task<Unit?> UpdateUnitAsync(
        Guid id,
        string number,
        string type,
        string? block = null,
        string? floor = null,
        decimal? area = null,
        int? bedrooms = null,
        int? bathrooms = null,
        int? parkingSpots = null,
        decimal? monthlyFee = null)
    {
        var tenantId = GetCurrentTenantId();
        var unitType = Enum.Parse<UnitType>(type, true);

        var unit = await _unitRepo.GetByIdAsync(id);
        if (unit == null || unit.CondominiumId != tenantId) return null;

        unit.Update(
            number,
            unitType,
            block: block,
            floor: floor,
            area: area,
            bedrooms: bedrooms,
            bathrooms: bathrooms,
            parkingSpots: parkingSpots);

        if (monthlyFee.HasValue)
            unit.UpdateFinancialInfo(monthlyFee.Value);

        _unitRepo.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return unit;
    }

    public async Task<bool> DeleteUnitAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();

        var unit = await _unitRepo.GetByIdAsync(id);
        if (unit == null || unit.CondominiumId != tenantId) return false;

        var allBills = await _billRepo.GetAllAsync();
        var hasPendingBills = allBills.Any(b =>
            b.UnitId == id && b.Status == BillStatus.Pending);

        if (hasPendingBills)
            throw new InvalidOperationException("Unidade possui faturas pendentes. Não pode ser removida.");

        unit.SoftDelete();
        _unitRepo.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<string>> GetBlocksAsync()
    {
        var tenantId = GetCurrentTenantId();

        var allUnits = await _unitRepo.GetAllAsync();
        return allUnits
            .Where(u => u.CondominiumId == tenantId && u.Block != null)
            .Select(u => u.Block!)
            .Distinct()
            .OrderBy(b => b)
            .ToList();
    }
}