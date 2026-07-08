using CondoSync.Core.Enums;

namespace CondoSync.Core.Entities;

public class User : AggregateRoot<Guid>, ITenantEntity
{
    public Guid CondominiumId { get; private set; }

    public string Name { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? Phone { get; private set; }
    public string? Cpf { get; private set; }
    public string? AvatarUrl { get; private set; }

    // Perfil (NUNCA super_admin - SuperAdmin fica em tabela separada)
    public UserRole Role { get; private set; }

    // Autenticação
    public DateTime? EmailVerifiedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }

    // Status
    public bool IsActive { get; private set; }

    // Preferências
    public string? NotificationPreferences { get; private set; }
    public string? ThemePreferences { get; private set; }

    // Refresh Token
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private User() { }

    public static User Create(
        Guid condominiumId,
        string name,
        string email,
        string passwordHash,
        UserRole role = UserRole.Resident,
        string? phone = null,
        string? cpf = null)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            CondominiumId = condominiumId,
            Name = name,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Phone = phone,
            Cpf = cpf,
            Role = role,
            IsActive = true,
            NotificationPreferences = "{\"email\": true, \"push\": true, \"in_app\": true}",
            ThemePreferences = "{\"mode\": \"light\", \"accent_color\": \"#1976D2\"}",
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

    public void UpdateProfile(string name, string? phone = null, string? avatarUrl = null)
    {
        Name = name;
        if (phone != null) Phone = phone;
        if (avatarUrl != null) AvatarUrl = avatarUrl;
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

    public void SetRefreshToken(string refreshToken, DateTime expiresAt)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
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

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }

    public void UpdateNotificationPreferences(string preferences)
    {
        NotificationPreferences = preferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateThemePreferences(string preferences)
    {
        ThemePreferences = preferences;
        UpdatedAt = DateTime.UtcNow;
    }
}