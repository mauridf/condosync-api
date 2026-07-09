using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using CondoSync.Application.Features.Auth.DTOs;
using CondoSync.Application.Services;

namespace CondoSync.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo condomínio e seu administrador
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginEndpoint")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var (user, accessToken, refreshToken) = await _authService.RegisterAsync(
                request.CondominiumName,
                request.CondominiumSlug,
                request.AdminName,
                request.AdminEmail,
                request.Password);

            var response = new AuthResponse(
                user.Id,
                user.Name,
                user.Email,
                user.Role.ToString(),
                user.CondominiumId,
                accessToken,
                refreshToken);

            _logger.LogInformation("Novo condomínio registrado: {Slug}", request.CondominiumSlug);

            return CreatedAtAction(nameof(Register), new { success = true, data = response });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { success = false, error = new { code = "CONFLICT", message = ex.Message } });
        }
    }

    /// <summary>
    /// Realiza login de usuário do condomínio
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginEndpoint")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (result == null)
        {
            return Unauthorized(new
            {
                success = false,
                error = new { code = "INVALID_CREDENTIALS", message = "Email ou senha inválidos" }
            });
        }

        var (user, accessToken, refreshToken) = result.Value;

        var response = new AuthResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.CondominiumId,
            accessToken,
            refreshToken);

        return Ok(new { success = true, data = response });
    }

    /// <summary>
    /// Renova o token de acesso usando refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        // O userId será extraído do token expirado ou do refresh token
        // Por simplicidade, vamos decodificar o refresh token
        // Em produção, implementar lógica completa de validação

        return Ok(new { success = true, message = "Refresh token endpoint - implementação completa pendente" });
    }

    /// <summary>
    /// Obtém o perfil do usuário autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("userId")?.Value;
        var name = User.FindFirst("name")?.Value;
        var email = User.FindFirst("email")?.Value;
        var role = User.FindFirst("role")?.Value;
        var tenantId = User.FindFirst("tenantId")?.Value;

        return Ok(new
        {
            success = true,
            data = new
            {
                userId,
                name,
                email,
                role,
                tenantId
            }
        });
    }

    /// <summary>
    /// Health check de autenticação
    /// </summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        return Ok(new { success = true, message = "Auth API funcionando", timestamp = DateTime.UtcNow });
    }
}