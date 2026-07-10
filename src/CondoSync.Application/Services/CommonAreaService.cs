using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.CommonAreas.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class CommonAreaService
{
    private readonly IRepository<CommonArea> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CommonAreaService> _logger;

    public CommonAreaService(
        IRepository<CommonArea> repository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<CommonAreaService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<CommonAreaResponse>> GetAllAsync(string? type = null, bool? isActive = null)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.CondominiumId == tenantId);

        var query = areas.AsQueryable();
        if (!string.IsNullOrEmpty(type))
            query = query.Where(a => a.Type == type);
        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        return query.OrderBy(a => a.Name).Select(MapToResponse).ToList();
    }

    public async Task<CommonAreaResponse?> GetByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        return areas.Select(MapToResponse).FirstOrDefault();
    }

    public async Task<CommonAreaResponse> CreateAsync(CreateCommonAreaRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var area = CommonArea.Create(tenantId, request.Name, request.Type,
            request.Description, request.Capacity, request.RequiresBooking);

        await _repository.AddAsync(area);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Área comum {Name} criada", request.Name);
        return MapToResponse(area);
    }

    public async Task<CommonAreaResponse?> UpdateAsync(Guid id, UpdateCommonAreaRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        var area = areas.FirstOrDefault();
        if (area == null) return null;

        area.Update(request.Name ?? area.Name, request.Description, request.Capacity, request.RequiresBooking);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(area);
    }

    public async Task<bool> SetMaintenanceAsync(Guid id, string description)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        var area = areas.FirstOrDefault();
        if (area == null) return false;

        area.SetMaintenance(description);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteMaintenanceAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        var area = areas.FirstOrDefault();
        if (area == null) return false;

        area.CompleteMaintenance();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetOperatingHoursAsync(Guid id, string operatingHours)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        var area = areas.FirstOrDefault();
        if (area == null) return false;

        area.SetOperatingHours(operatingHours);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var areas = await _repository.FindAsync(a => a.Id == id && a.CondominiumId == tenantId);
        var area = areas.FirstOrDefault();
        if (area == null) return false;

        _repository.Remove(area);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static CommonAreaResponse MapToResponse(CommonArea a)
    {
        return new CommonAreaResponse(
            a.Id, a.Name, a.Description, a.Type, a.Capacity,
            a.MaxGuestsPerResident, a.RequiresBooking, a.RequiresDeposit,
            a.DepositAmount, a.OperatingHours, a.IsActive,
            a.MaintenanceStatus, a.ScheduledMaintenance, a.Images,
            a.CreatedAt, a.UpdatedAt);
    }
}
