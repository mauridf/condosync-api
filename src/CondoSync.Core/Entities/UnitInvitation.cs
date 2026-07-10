using CondoSync.Core.Exceptions;

namespace CondoSync.Core.Entities;

public class UnitInvitation : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }
    public Guid UnitId { get; private set; }

    public string InvitationCode { get; private set; } = default!;
    public string? InvitationUrl { get; private set; }

    // Validade
    public DateTime? ExpiresAt { get; private set; }
    public int MaxUses { get; private set; }
    public int UsesCount { get; private set; }

    // Destinatário
    public string? RecipientEmail { get; private set; }
    public string? RecipientName { get; private set; }
    public string? RecipientPhone { get; private set; }

    // Tipo de acesso
    public string AccessType { get; private set; } = default!;

    // Status
    public string Status { get; private set; } = default!;

    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private UnitInvitation() { }

    public static UnitInvitation Create(
        Guid condominiumId,
        Guid unitId,
        Guid createdBy,
        string? recipientEmail = null,
        string? recipientName = null,
        string? recipientPhone = null,
        string accessType = "owner",
        int maxUses = 1,
        int validityDays = 7)
    {
        var invitation = new UnitInvitation
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            UnitId = unitId,
            CreatedBy = createdBy,
            InvitationCode = GenerateInvitationCode(),
            RecipientEmail = recipientEmail?.ToLowerInvariant(),
            RecipientName = recipientName,
            RecipientPhone = recipientPhone,
            AccessType = accessType,
            MaxUses = maxUses,
            ExpiresAt = DateTime.UtcNow.AddDays(validityDays),
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        invitation.InvitationUrl = $"/invite/{invitation.InvitationCode}";

        return invitation;
    }

    private static string GenerateInvitationCode()
    {
        return Guid.NewGuid().ToString("N")[..12].ToUpper();
    }

    public void Use()
    {
        if (Status != "active")
            throw new DomainException("Convite não está ativo");

        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            throw new DomainException("Convite expirado");

        if (UsesCount >= MaxUses)
            throw new DomainException("Convite já atingiu o limite de usos");

        UsesCount++;

        if (UsesCount >= MaxUses)
            Status = "used";

        UpdatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (Status == "used")
            throw new DomainException("Convite já foi utilizado");

        Status = "revoked";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != "active") return;

        Status = "expired";
        UpdatedAt = DateTime.UtcNow;
    }
}