using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Common.Interfaces;
using CondoSync.Application.Features.Employees.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class EmployeeService
{
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Resident> _residentRepo;
    private readonly IRepository<Unit> _unitRepo;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IRepository<User> userRepo,
        IRepository<Resident> residentRepo,
        IRepository<Unit> unitRepo,
        ITenantProvider tenantProvider,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeService> logger)
    {
        _userRepo = userRepo;
        _residentRepo = residentRepo;
        _unitRepo = unitRepo;
        _tenantProvider = tenantProvider;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var existing = await _userRepo.FindAsync(u =>
            u.Email == request.Email && u.CondominiumId == tenantId && u.IsActive);
        if (existing.Any())
            throw new InvalidOperationException("EMAIL_ALREADY_EXISTS");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var roleEnum)
            || roleEnum is not (UserRole.Employee or UserRole.SubAdmin))
        {
            throw new InvalidOperationException("INVALID_ROLE");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = User.Create(
            tenantId,
            request.Name,
            request.Email,
            passwordHash,
            roleEnum,
            phone: request.Phone);

        await _userRepo.AddAsync(user);

        Guid unitId;
        if (request.UnitId.HasValue)
        {
            var units = await _unitRepo.FindAsync(u => u.Id == request.UnitId && u.CondominiumId == tenantId);
            if (!units.Any())
                throw new InvalidOperationException("UNIT_NOT_FOUND");
            unitId = request.UnitId.Value;
        }
        else
        {
            var units = await _unitRepo.FindAsync(u => u.CondominiumId == tenantId && u.IsActive);
            var firstUnit = units.FirstOrDefault();
            unitId = firstUnit?.Id ?? Guid.Empty;
            if (unitId == Guid.Empty)
                throw new InvalidOperationException("NO_UNITS_IN_CONDOMINIUM");
        }

        var resident = Resident.Create(
            tenantId,
            unitId,
            request.Name,
            ResidentType.Employee,
            email: request.Email,
            phone: request.Phone,
            cpf: request.Document);

        resident.LinkUser(user.Id);

        await _residentRepo.AddAsync(resident);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Funcionário criado: {Email} no condomínio {TenantId}", request.Email, tenantId);

        return new EmployeeResponse(
            resident.Id,
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            resident.Cpf,
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt);
    }

    public async Task<List<EmployeeResponse>> GetEmployeesAsync()
    {
        var tenantId = GetCurrentTenantId();

        var residents = await _residentRepo.FindAsync(r =>
            r.CondominiumId == tenantId && r.ResidentType == ResidentType.Employee && r.IsActive);

        var userIds = residents.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).ToHashSet();
        var users = await _userRepo.FindAsync(u => userIds.Contains(u.Id) && u.CondominiumId == tenantId);
        var usersDict = users.ToDictionary(u => u.Id);

        return residents.Select(r =>
        {
            var user = r.UserId.HasValue ? usersDict.GetValueOrDefault(r.UserId.Value) : null;
            return new EmployeeResponse(
                r.Id,
                r.UserId ?? Guid.Empty,
                r.Name,
                r.Email ?? user?.Email ?? "—",
                r.Phone,
                r.Cpf,
                user?.Role.ToString() ?? "Employee",
                r.IsActive,
                r.CreatedAt);
        }).OrderBy(e => e.Name).ToList();
    }
}
