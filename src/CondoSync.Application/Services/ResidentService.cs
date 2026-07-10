using System.Linq.Expressions;
using CondoSync.Core.Common;
using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Residents.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class ResidentService
{
    private readonly IRepository<Resident> _residentRepo;
    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<CondominiumSettings> _settingsRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResidentService> _logger;

    public ResidentService(
        IRepository<Resident> residentRepo,
        IRepository<Unit> unitRepo,
        IRepository<CondominiumSettings> settingsRepo,
        ITenantProvider tenantProvider,
        IUnitOfWork unitOfWork,
        ILogger<ResidentService> logger)
    {
        _residentRepo = residentRepo;
        _unitRepo = unitRepo;
        _settingsRepo = settingsRepo;
        _tenantProvider = tenantProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<Resident>> GetResidentsAsync(
        Guid? unitId = null,
        string? residentType = null,
        string? search = null,
        int page = 1,
        int perPage = 20)
    {
        var tenantId = GetCurrentTenantId();
        var allResidents = (await _residentRepo.GetAllAsync())
            .Where(r => r.CondominiumId == tenantId && r.IsActive);

        if (unitId.HasValue)
            allResidents = allResidents.Where(r => r.UnitId == unitId.Value);

        if (!string.IsNullOrWhiteSpace(residentType))
        {
            var type = Enum.Parse<ResidentType>(residentType, true);
            allResidents = allResidents.Where(r => r.ResidentType == type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            allResidents = allResidents.Where(r => r.Name.Contains(search) ||
                (r.Email != null && r.Email.Contains(search)) ||
                (r.Cpf != null && r.Cpf.Contains(search)));
        }

        return allResidents
            .OrderBy(r => r.Name)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Resident?> GetResidentByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(id);
        return resident?.CondominiumId == tenantId ? resident : null;
    }

    public async Task<List<Resident>> GetResidentsByUnitAsync(Guid unitId)
    {
        var tenantId = GetCurrentTenantId();
        var allResidents = await _residentRepo.GetAllAsync();
        return allResidents
            .Where(r => r.CondominiumId == tenantId && r.UnitId == unitId && r.IsActive)
            .OrderByDescending(r => r.IsPrimary)
            .ThenBy(r => r.Name)
            .ToList();
    }

    public async Task<Resident> CreateResidentAsync(CreateResidentRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var residentType = Enum.Parse<ResidentType>(request.ResidentType, true);

        var allUnits = await _unitRepo.GetAllAsync();
        var unit = allUnits.FirstOrDefault(u => u.Id == request.UnitId && u.CondominiumId == tenantId);
        if (unit == null)
            throw new InvalidOperationException("Unidade não encontrada");

        var settings = (await _settingsRepo.GetAllAsync())
            .FirstOrDefault(s => s.CondominiumId == tenantId);

        if (settings != null)
        {
            var allResidents = await _residentRepo.GetAllAsync();
            var currentCount = allResidents.Count(r => r.UnitId == request.UnitId && r.IsActive);
            if (currentCount >= settings.MaxFamilyMembersPerUnit)
                throw new InvalidOperationException($"Limite de {settings.MaxFamilyMembersPerUnit} moradores por unidade atingido");
        }

        if (request.IsPrimary)
        {
            var allResidents = await _residentRepo.GetAllAsync();
            var existingPrimary = allResidents
                .Where(r => r.UnitId == request.UnitId && r.IsPrimary && r.IsActive)
                .ToList();

            foreach (var ep in existingPrimary)
            {
                ep.UnsetPrimary();
            }
        }

        var resident = Resident.Create(
            tenantId,
            request.UnitId,
            request.Name,
            residentType,
            email: request.Email,
            phone: request.Phone,
            cpf: request.Cpf,
            isPrimary: request.IsPrimary);

        if (residentType == ResidentType.Tenant && !string.IsNullOrWhiteSpace(request.OwnerName))
        {
            resident.SetOwnerInfo(request.OwnerName, request.OwnerPhone, request.OwnerEmail);
        }

        if (request.IsEmergencyContact)
            resident.SetAsEmergencyContact();

        if (request.Vehicles != null)
        {
            foreach (var vehicle in request.Vehicles)
            {
                resident.AddVehicle(vehicle.Plate, vehicle.Model, vehicle.Color, vehicle.Brand);
            }
        }

        if (request.Pets != null)
        {
            foreach (var pet in request.Pets)
            {
                resident.AddPet(pet.Name, pet.Species, pet.Breed, pet.Color);
            }
        }

        await _residentRepo.AddAsync(resident);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Morador criado: {Name} na unidade {UnitId}", request.Name, request.UnitId);

        return resident;
    }

    public async Task<Resident?> UpdateResidentAsync(Guid id, UpdateResidentRequest request)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(id);
        if (resident == null || resident.CondominiumId != tenantId) return null;

        resident.Update(request.Name, request.Email, request.Phone, request.Profession);

        if (request.IsEmergencyContact.HasValue && request.IsEmergencyContact.Value)
            resident.SetAsEmergencyContact();

        _residentRepo.Update(resident);
        await _unitOfWork.SaveChangesAsync();

        return resident;
    }

    public async Task<VehicleEntry?> AddVehicleAsync(Guid residentId, string plate, string model, string? color, string? brand)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return null;

        resident.AddVehicle(plate, model, color, brand);
        _residentRepo.Update(resident);
        await _unitOfWork.SaveChangesAsync();
        return resident.GetVehicles().LastOrDefault();
    }

    public async Task<bool> RemoveVehicleAsync(Guid residentId, Guid vehicleId)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        var removed = resident.RemoveVehicle(vehicleId);
        if (removed)
        {
            _residentRepo.Update(resident);
            await _unitOfWork.SaveChangesAsync();
        }
        return removed;
    }

    public async Task<bool> UpdateVehicleAsync(Guid residentId, Guid vehicleId, string plate, string model, string? color, string? brand)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        var updated = resident.UpdateVehicle(vehicleId, plate, model, color, brand);
        if (updated)
        {
            _residentRepo.Update(resident);
            await _unitOfWork.SaveChangesAsync();
        }
        return updated;
    }

    // ─── Pets ───────────────────────────────────────────────────────────

    public async Task<PetEntry?> AddPetAsync(Guid residentId, string name, string species, string? breed, string? color)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return null;

        resident.AddPet(name, species, breed, color);
        _residentRepo.Update(resident);
        await _unitOfWork.SaveChangesAsync();
        return resident.GetPets().LastOrDefault();
    }

    public async Task<bool> RemovePetAsync(Guid residentId, Guid petId)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        var removed = resident.RemovePet(petId);
        if (removed)
        {
            _residentRepo.Update(resident);
            await _unitOfWork.SaveChangesAsync();
        }
        return removed;
    }

    public async Task<bool> UpdatePetAsync(Guid residentId, Guid petId, string name, string species, string? breed, string? color)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(residentId);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        var updated = resident.UpdatePet(petId, name, species, breed, color);
        if (updated)
        {
            _residentRepo.Update(resident);
            await _unitOfWork.SaveChangesAsync();
        }
        return updated;
    }

    public async Task<bool> DeleteResidentAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(id);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        resident.MoveOut(DateOnly.FromDateTime(DateTime.UtcNow));
        resident.SoftDelete();
        _residentRepo.Update(resident);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleAccessAsync(Guid id, bool grantAccess)
    {
        var tenantId = GetCurrentTenantId();
        var resident = await _residentRepo.GetByIdAsync(id);
        if (resident == null || resident.CondominiumId != tenantId) return false;

        if (grantAccess)
        {
            var accessCode = Guid.NewGuid().ToString()[..8].ToUpper();
            resident.GrantSystemAccess(accessCode);
        }
        else
        {
            resident.RevokeSystemAccess();
        }

        _residentRepo.Update(resident);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}