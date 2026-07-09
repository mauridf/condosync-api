using CondoSync.Core.Entities;
using CondoSync.Core.Enums;
using CondoSync.Core.Interfaces;
using CondoSync.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CondoSync.Application.Services;

public class AuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Condominium> _condominiumRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<Condominium> condominiumRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _condominiumRepository = condominiumRepository;
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
        Guid userId, string refreshToken)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || user.RefreshToken != refreshToken)
            return null;

        if (user.RefreshTokenExpiresAt < DateTime.UtcNow)
            return null;

        var (accessToken, newRefreshToken) = _tokenService.GenerateTokenPair(
            user.Id, user.CondominiumId, user.Role.ToString(), user.Name, user.Email);

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync();

        return (accessToken, newRefreshToken);
    }
}