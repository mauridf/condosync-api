using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class ServiceManagementService
{
    private readonly IRepository<Service> _serviceRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ServiceManagementService> _logger;

    public ServiceManagementService(
        IRepository<Service> serviceRepo,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork,
        ILogger<ServiceManagementService> logger)
    {
        _serviceRepo = serviceRepo;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Service>> GetServicesAsync(bool? isActive = null)
    {
        var tenantId = GetCurrentTenantId();
        var allServices = (await _serviceRepo.GetAllAsync())
            .Where(s => s.CondominiumId == tenantId);

        if (isActive.HasValue)
            allServices = allServices.Where(s => s.IsActive == isActive.Value);

        return allServices
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Name)
            .ToList();
    }

    public async Task<Service?> GetServiceByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var service = await _serviceRepo.GetByIdAsync(id);
        return service?.CondominiumId == tenantId ? service : null;
    }

    public async Task<Service> CreateServiceAsync(
        string name, string slug, string category, string serviceType,
        string? description = null, decimal price = 0,
        bool requiresApproval = false, bool requiresPayment = false)
    {
        var tenantId = GetCurrentTenantId();
        var type = Enum.Parse<ServiceType>(serviceType, true);

        var allServices = await _serviceRepo.GetAllAsync();
        if (allServices.Any(s => s.CondominiumId == tenantId && s.Slug == slug))
            throw new InvalidOperationException("Slug já está em uso");

        var service = Service.Create(
            tenantId, name, slug, category, type,
            description, price, requiresApproval, requiresPayment);

        await _serviceRepo.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();

        return service;
    }

    public async Task<Service?> UpdateServiceAsync(Guid id, string name,
        string? description = null, decimal? price = null,
        bool? requiresApproval = null)
    {
        var tenantId = GetCurrentTenantId();
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service == null || service.CondominiumId != tenantId) return null;

        service.Update(name, description, price, requiresApproval);
        _serviceRepo.Update(service);
        await _unitOfWork.SaveChangesAsync();

        return service;
    }

    public async Task<bool> ToggleServiceAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service == null || service.CondominiumId != tenantId) return false;

        service.ToggleActive();
        _serviceRepo.Update(service);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteServiceAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service == null || service.CondominiumId != tenantId) return false;

        service.SoftDelete();
        _serviceRepo.Update(service);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}