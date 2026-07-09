using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CondoSync.Application.Common.Interfaces;
using CondoSync.Application.Features.Auth.DTOs;
using CondoSync.Infrastructure.Data;

namespace CondoSync.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly AdminDbContext _adminDbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AdminAuthController> _logger;

    public AdminAuthController(
        AdminDbContext adminDbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AdminAuthController> logger)
    {
        _adminDbContext = adminDbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login do SuperAdmin (domínio global - sem tenant)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginEndpoint")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        var admin = await _adminDbContext.SuperAdmins
            .FirstOrDefaultAsync(sa => sa.Email == request.Email && sa.IsActive);

        if (admin == null)
        {
            _logger.LogWarning("Tentativa de login admin com email não encontrado: {Email}", request.Email);
            return Unauthorized(new
            {
                success = false,
                error = new { code = "INVALID_CREDENTIALS", message = "Credenciais inválidas" }
            });
        }

        if (admin.IsLockedOut())
        {
            return StatusCode(423, new
            {
                success = false,
                error = new { code = "ACCOUNT_LOCKED", message = "Conta bloqueada. Tente novamente em 15 minutos." }
            });
        }

        if (!_passwordHasher.VerifyPassword(request.Password, admin.PasswordHash))
        {
            admin.RecordFailedLogin();
            await _adminDbContext.SaveChangesAsync();

            _logger.LogWarning("Senha incorreta para admin: {Email}", request.Email);
            return Unauthorized(new
            {
                success = false,
                error = new { code = "INVALID_CREDENTIALS", message = "Credenciais inválidas" }
            });
        }

        // Login bem-sucedido
        admin.RecordLogin();
        await _adminDbContext.SaveChangesAsync();

        var (accessToken, refreshToken) = _tokenService.GenerateAdminTokenPair(
            admin.Id, admin.Role.ToString(), admin.Name, admin.Email);

        var response = new AdminAuthResponse(
            admin.Id,
            admin.Name,
            admin.Email,
            admin.Role.ToString(),
            accessToken,
            refreshToken);

        _logger.LogInformation("Login admin bem-sucedido: {Email} ({Role})", request.Email, admin.Role);

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Obtém o perfil do SuperAdmin autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Admin", Roles = "super_admin,support,analyst")]
    public async Task<IActionResult> GetCurrentAdmin()
    {
        var adminId = User.FindFirst("adminId")?.Value;

        if (string.IsNullOrEmpty(adminId) || !Guid.TryParse(adminId, out var id))
            return Unauthorized();

        var admin = await _adminDbContext.SuperAdmins.FindAsync(id);

        if (admin == null)
            return NotFound(new { success = false, error = new { code = "NOT_FOUND", message = "Admin não encontrado" } });

        return Ok(new
        {
            success = true,
            data = new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Role,
                admin.IsActive,
                admin.LastLoginAt
            }
        });
    }

    /// <summary>
    /// Health check da autenticação admin
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(new { success = true, message = "Admin Auth API funcionando", timestamp = DateTime.UtcNow });
    }
}