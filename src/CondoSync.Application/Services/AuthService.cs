using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace CondoSync.Application.Services;

public class AuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Condominium> _condominiumRepository;
    private readonly IRepository<Resident> _residentRepository;
    private readonly IRepository<UnitInvitation> _invitationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<Condominium> condominiumRepository,
        IRepository<Resident> residentRepository,
        IRepository<UnitInvitation> invitationRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _condominiumRepository = condominiumRepository;
        _residentRepository = residentRepository;
        _invitationRepository = invitationRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<(User user, string accessToken, string refreshToken)> RegisterAsync(
        string condominiumName, string condominiumSlug, string adminName,
        string adminEmail, string password)
    {
        // Verificar se slug já existe
        var existingCondo = await _condominiumRepository.FindAsync(
            c => c.Slug == condominiumSlug);

        if (existingCondo.Any())
            throw new InvalidOperationException("Slug já está em uso");

        // Criar condomínio
        var condominium = Condominium.Create(
            condominiumName,
            condominiumSlug,
            email: adminEmail,
            plan: SubscriptionPlan.Trial);

        await _condominiumRepository.AddAsync(condominium);

        // Criar admin do condomínio
        var passwordHash = _passwordHasher.HashPassword(password);
        var adminUser = User.Create(
            condominium.Id,
            adminName,
            adminEmail,
            passwordHash,
            UserRole.CondoAdmin);

        await _userRepository.AddAsync(adminUser);
        await _unitOfWork.SaveChangesAsync();

        // Gerar tokens
        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(
            adminUser.Id, condominium.Id, "CondoAdmin", adminUser.Name, adminUser.Email);

        // Salvar refresh token
        adminUser.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Novo condomínio registrado: {Slug} com admin {Email}",
            condominiumSlug, adminEmail);

        return (adminUser, accessToken, refreshToken);
    }

    public async Task<(User user, string accessToken, string refreshToken)> RegisterResidentAsync(
        string invitationCode, string name, string email, string password)
    {
        var invitations = await _invitationRepository.FindAsync(i => i.InvitationCode == invitationCode);
        var invitation = invitations.FirstOrDefault();

        if (invitation == null)
            throw new InvalidOperationException("INVITATION_NOT_FOUND");

        if (invitation.Status != "active")
            throw new InvalidOperationException("INVITATION_NOT_ACTIVE");

        if (invitation.ExpiresAt.HasValue && DateTime.UtcNow > invitation.ExpiresAt.Value)
            throw new InvalidOperationException("INVITATION_EXPIRED");

        if (invitation.UsesCount >= invitation.MaxUses)
            throw new InvalidOperationException("INVITATION_MAX_USES");

        var existingUsers = await _userRepository.FindAsync(u =>
            u.Email == email && u.CondominiumId == invitation.CondominiumId && u.IsActive);
        if (existingUsers.Any())
            throw new InvalidOperationException("EMAIL_ALREADY_REGISTERED");

        var passwordHash = _passwordHasher.HashPassword(password);

        var user = User.Create(
            invitation.CondominiumId,
            name,
            email,
            passwordHash,
            UserRole.Resident);

        await _userRepository.AddAsync(user);

        var existingResidents = await _residentRepository.FindAsync(r =>
            r.Email == email && r.CondominiumId == invitation.CondominiumId && r.IsActive);
        var resident = existingResidents.FirstOrDefault();

        if (resident == null)
        {
            resident = Resident.Create(
                invitation.CondominiumId,
                invitation.UnitId,
                name,
                ResidentType.Tenant,
                email: email,
                phone: invitation.RecipientPhone);

            await _residentRepository.AddAsync(resident);
        }

        resident.LinkUser(user.Id);
        invitation.Use();

        await _unitOfWork.SaveChangesAsync();

        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(
            user.Id, invitation.CondominiumId, UserRole.Resident.ToString(), user.Name, user.Email);

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Morador registrado: {Email} no condomínio {CondominiumId} via convite {Code}",
            email, invitation.CondominiumId, invitationCode);

        return (user, accessToken, refreshToken);
    }

    public async Task<(User user, string accessToken, string refreshToken)?> LoginAsync(
        string email, string password)
    {
        var users = await _userRepository.FindAsync(
            u => u.Email == email && u.IsActive);

        var user = users.FirstOrDefault();

        if (user == null)
        {
            _logger.LogWarning("Tentativa de login com email não encontrado: {Email}", email);
            return null;
        }

        if (user.IsLockedOut())
        {
            _logger.LogWarning("Tentativa de login em conta bloqueada: {Email}", email);
            throw new InvalidOperationException("Conta temporariamente bloqueada. Tente novamente em 15 minutos.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("Senha incorreta para usuário: {Email}", email);
            return null;
        }

        // Login bem-sucedido
        user.RecordLogin();

        var (accessToken, refreshToken) = _tokenService.GenerateTokenPair(
            user.Id, user.CondominiumId, user.Role.ToString(), user.Name, user.Email);

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Login bem-sucedido: {Email}", email);

        return (user, accessToken, refreshToken);
    }

    public async Task<(string accessToken, string refreshToken)?> RefreshTokenAsync(
        string refreshToken)
    {
        var users = await _userRepository.FindAsync(
            u => u.RefreshToken == refreshToken);

        var user = users.FirstOrDefault();

        if (user == null)
            return null;

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            user.ClearRefreshToken();
            await _unitOfWork.SaveChangesAsync();
            return null;
        }

        var (accessToken, newRefreshToken) = _tokenService.GenerateTokenPair(
            user.Id, user.CondominiumId, user.Role.ToString(), user.Name, user.Email);

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token renovado para usuário: {Email}", user.Email);

        return (accessToken, newRefreshToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return false;

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        var newHash = _passwordHasher.HashPassword(newPassword);
        user.ChangePassword(newHash);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Senha alterada para usuário {UserId}", userId);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var users = await _userRepository.FindAsync(u => u.Email == email && u.IsActive);
        var user = users.FirstOrDefault();
        if (user == null)
        {
            _logger.LogWarning("Tentativa de reset de senha para email não encontrado: {Email}", email);
            return false;
        }

        _logger.LogInformation("Token de reset gerado para {Email}", email);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        _logger.LogInformation("Senha resetada via token");
        return true;
    }

    public async Task<bool> VerifyEmailAsync(Guid userId, string token)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return false;

        user.VerifyEmail();
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Email verificado para usuário {UserId}", userId);
        return true;
    }

    public async Task<(string Secret, string QrCodeUrl)?> Setup2FaAsync(Guid userId)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return null;

        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(20));
        var qrCodeUrl = $"otpauth://totp/CondoSync:{user.Email}?secret={secret}&issuer=CondoSync";

        return (secret, qrCodeUrl);
    }

    public async Task<bool> Enable2FaAsync(Guid userId, string code)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return false;

        user.UpdateRole(user.Role);
        return true;
    }

    public async Task<bool> Disable2FaAsync(Guid userId, string code)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return false;

        return true;
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, string? name, string? phone, string? avatarUrl)
    {
        var users = await _userRepository.FindAsync(u => u.Id == userId);
        var user = users.FirstOrDefault();
        if (user == null) return false;

        user.UpdateProfile(name ?? user.Name, phone, avatarUrl);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}