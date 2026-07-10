using CondoSync.Core.Entities;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Features.Invitations.DTOs;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class UnitInvitationService
{
    private readonly IRepository<UnitInvitation> _invitationRepository;
    private readonly IRepository<Unit> _unitRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UnitInvitationService> _logger;

    public UnitInvitationService(
        IRepository<UnitInvitation> invitationRepository,
        IRepository<Unit> unitRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ILogger<UnitInvitationService> logger)
    {
        _invitationRepository = invitationRepository;
        _unitRepository = unitRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    private Guid GetCurrentTenantId()
    {
        return _tenantProvider.GetCurrentTenantId()
            ?? throw new UnauthorizedAccessException("Tenant não identificado");
    }

    public async Task<List<InvitationResponse>> GetAllAsync(Guid? unitId = null, string? status = null)
    {
        var tenantId = GetCurrentTenantId();
        var invitations = await _invitationRepository.FindAsync(i => i.CondominiumId == tenantId);

        var query = invitations.AsQueryable();
        if (unitId.HasValue)
            query = query.Where(i => i.UnitId == unitId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);

        return query.OrderByDescending(i => i.CreatedAt).Select(MapToResponse).ToList();
    }

    public async Task<InvitationResponse?> GetByIdAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var invitations = await _invitationRepository.FindAsync(i => i.Id == id && i.CondominiumId == tenantId);
        return invitations.Select(MapToResponse).FirstOrDefault();
    }

    public async Task<InvitationResponse> CreateAsync(Guid createdBy, CreateInvitationRequest request)
    {
        var tenantId = GetCurrentTenantId();

        var units = await _unitRepository.FindAsync(u => u.Id == request.UnitId && u.CondominiumId == tenantId);
        if (!units.Any())
            throw new InvalidOperationException("Unidade não encontrada");

        var invitation = UnitInvitation.Create(
            tenantId, request.UnitId, createdBy,
            request.RecipientEmail, request.RecipientName,
            request.RecipientPhone, request.AccessType,
            request.MaxUses, request.ValidityDays);

        await _invitationRepository.AddAsync(invitation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Convite {Code} criado para unidade {UnitId}", invitation.InvitationCode, request.UnitId);
        return MapToResponse(invitation);
    }

    public async Task<InvitationResponse?> UseAsync(string invitationCode)
    {
        var invitations = await _invitationRepository.FindAsync(i => i.InvitationCode == invitationCode);
        var invitation = invitations.FirstOrDefault();
        if (invitation == null) return null;

        invitation.Use();
        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(invitation);
    }

    public async Task<bool> RevokeAsync(Guid id)
    {
        var tenantId = GetCurrentTenantId();
        var invitations = await _invitationRepository.FindAsync(i => i.Id == id && i.CondominiumId == tenantId);
        var invitation = invitations.FirstOrDefault();
        if (invitation == null) return false;

        invitation.Revoke();
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static InvitationResponse MapToResponse(UnitInvitation i)
    {
        return new InvitationResponse(
            i.Id, i.UnitId, i.InvitationCode, i.InvitationUrl,
            i.RecipientEmail, i.RecipientName, i.RecipientPhone,
            i.AccessType, i.Status, i.MaxUses, i.UsesCount,
            i.ExpiresAt, i.CreatedBy, i.CreatedAt, i.UpdatedAt);
    }
}
