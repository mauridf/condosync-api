using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class AdminService
{
    private readonly IRepository<Condominium> _condominiumRepo;
    private readonly IRepository<User> _userRepo;
    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<Resident> _residentRepo;
    private readonly IRepository<Ticket> _ticketRepo;
    private readonly IRepository<Bill> _billRepo;
    private readonly IRepository<CondominiumSettings> _settingsRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IRepository<Condominium> condominiumRepo,
        IRepository<User> userRepo,
        IRepository<Unit> unitRepo,
        IRepository<Resident> residentRepo,
        IRepository<Ticket> ticketRepo,
        IRepository<Bill> billRepo,
        IRepository<CondominiumSettings> settingsRepo,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<AdminService> logger)
    {
        _condominiumRepo = condominiumRepo;
        _userRepo = userRepo;
        _unitRepo = unitRepo;
        _residentRepo = residentRepo;
        _ticketRepo = ticketRepo;
        _billRepo = billRepo;
        _settingsRepo = settingsRepo;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<List<Condominium>> GetAllCondominiumsAsync(
        string? search = null,
        string? status = null,
        string? plan = null,
        int page = 1,
        int perPage = 20)
    {
        var query = (await _condominiumRepo.GetAllAsync()).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Name.Contains(search) || c.Slug.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(c => c.SubscriptionStatus.ToString() == status);
        }

        if (!string.IsNullOrWhiteSpace(plan))
        {
            query = query.Where(c => c.SubscriptionPlan.ToString() == plan);
        }

        return query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();
    }

    public async Task<Condominium?> GetCondominiumByIdAsync(Guid id)
    {
        return await _condominiumRepo.GetByIdAsync(id);
    }

    public async Task<Condominium> CreateCondominiumAsync(
        string name,
        string slug,
        string adminName,
        string adminEmail,
        string adminPassword,
        string? cnpj = null,
        string? phone = null,
        string plan = "trial")
    {
        var allCondos = await _condominiumRepo.GetAllAsync();
        if (allCondos.Any(c => c.Slug == slug))
            throw new InvalidOperationException("Slug já está em uso");

        var subscriptionPlan = Enum.Parse<SubscriptionPlan>(plan, true);

        var condominium = Condominium.Create(
            name,
            slug,
            cnpj: cnpj,
            email: adminEmail,
            phone: phone,
            plan: subscriptionPlan);

        await _condominiumRepo.AddAsync(condominium);

        var passwordHash = _passwordHasher.HashPassword(adminPassword);
        var adminUser = User.Create(
            condominium.Id,
            adminName,
            adminEmail,
            passwordHash,
            UserRole.CondoAdmin);

        await _userRepo.AddAsync(adminUser);

        var settings = CondominiumSettings.CreateDefault(condominium.Id);
        await _settingsRepo.AddAsync(settings);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Condomínio criado pelo admin: {Slug}", slug);

        return condominium;
    }

    public async Task<Condominium?> UpdateCondominiumAsync(
        Guid id,
        string name,
        string? email = null,
        string? phone = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? zipCode = null)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);

        if (condominium == null)
            return null;

        condominium.Update(name, email, phone);

        if (address != null || city != null || state != null || zipCode != null)
            condominium.UpdateAddress(address, city, state, zipCode);

        _condominiumRepo.Update(condominium);
        await _unitOfWork.SaveChangesAsync();

        return condominium;
    }

    public async Task<bool> SuspendCondominiumAsync(Guid id)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);

        if (condominium == null)
            return false;

        condominium.Suspend();
        _condominiumRepo.Update(condominium);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Condomínio suspenso: {Id}", id);

        return true;
    }

    public async Task<bool> ActivateCondominiumAsync(Guid id)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);

        if (condominium == null)
            return false;

        condominium.Activate();
        _condominiumRepo.Update(condominium);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Condomínio ativado: {Id}", id);

        return true;
    }

    public async Task<bool> ChangePlanAsync(Guid id, string plan)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);

        if (condominium == null)
            return false;

        var subscriptionPlan = Enum.Parse<SubscriptionPlan>(plan, true);
        condominium.ChangePlan(subscriptionPlan);
        _condominiumRepo.Update(condominium);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Plano alterado para {Plan}: {Id}", plan, id);

        return true;
    }

    public async Task<bool> DeleteCondominiumAsync(Guid id)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);

        if (condominium == null)
            return false;

        condominium.SoftDelete();
        _condominiumRepo.Update(condominium);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning("Condomínio removido (soft delete): {Id}", id);

        return true;
    }

    public async Task<object?> GetCondominiumUsageAsync(Guid id)
    {
        var condominium = await _condominiumRepo.GetByIdAsync(id);
        if (condominium == null) return null;

        var allUnits = await _unitRepo.GetAllAsync();
        var allResidents = await _residentRepo.GetAllAsync();
        var allUsers = await _userRepo.GetAllAsync();
        var allTickets = await _ticketRepo.GetAllAsync();
        var allBills = await _billRepo.GetAllAsync();

        var totalUnits = allUnits.Count(u => u.CondominiumId == id);
        var totalResidents = allResidents.Count(r => r.CondominiumId == id);
        var totalUsers = allUsers.Count(u => u.CondominiumId == id);
        var openTickets = allTickets.Count(t => t.CondominiumId == id && t.Status == TicketStatus.Open);
        var pendingBills = allBills.Count(b => b.CondominiumId == id && b.Status == BillStatus.Pending);

        return new
        {
            CondominiumId = id,
            TotalUnits = totalUnits,
            TotalResidents = totalResidents,
            TotalUsers = totalUsers,
            OpenTickets = openTickets,
            PendingBills = pendingBills,
            MaxUnits = condominium.MaxUnits,
            MaxResidentsPerUnit = condominium.MaxResidentsPerUnit
        };
    }
}