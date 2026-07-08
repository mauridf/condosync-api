using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class SuperAdmin : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public SuperAdminRole Role { get; private set; }

    // Autenticação
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Construtor privado para EF Core
    private SuperAdmin() { }

    public static SuperAdmin Create(string name, string email, string passwordHash, SuperAdminRole role = SuperAdminRole.SuperAdmin)
    {
        return new SuperAdmin
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        LastPasswordChangeAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        EmailVerifiedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }
}