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
    /// Registra um novo morador via convite
    /// </summary>
    [HttpPost("register-resident")]
    [AllowAnonymous]
    [EnableRateLimiting("LoginEndpoint")]
    public async Task<IActionResult> RegisterResident([FromBody] RegisterResidentRequest request)
    {
        try
        {
            var (user, accessToken, refreshToken) = await _authService.RegisterResidentAsync(
                request.InvitationCode,
                request.Name,
                request.Email,
                request.Password);

            var response = new AuthResponse(
                user.Id,
                user.Name,
                user.Email,
                user.Role.ToString(),
                user.CondominiumId,
                accessToken,
                refreshToken);

            _logger.LogInformation("Morador registrado via convite: {Email}", request.Email);

            return Ok(new { success = true, data = response });
        }
        catch (InvalidOperationException ex)
        {
            var code = ex.Message;
            var message = code switch
            {
                "INVITATION_NOT_FOUND" => "Convite não encontrado",
                "INVITATION_NOT_ACTIVE" => "Convite não está ativo",
                "INVITATION_EXPIRED" => "Convite expirado",
                "INVITATION_MAX_USES" => "Convite já atingiu o limite de usos",
                "EMAIL_ALREADY_REGISTERED" => "Este email já está registrado neste condomínio",
                _ => ex.Message
            };
            return BadRequest(new { success = false, error = new { code, message } });
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
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (result == null)
        {
            return Unauthorized(new
            {
                success = false,
                error = new { code = "INVALID_REFRESH_TOKEN", message = "Refresh token inválido ou expirado" }
            });
        }

        var (accessToken, refreshToken) = result.Value;

        return Ok(new
        {
            success = true,
            data = new TokenResponse(accessToken, refreshToken)
        });
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

    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }

    /// <summary>
    /// Altera a senha do usuário autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Senha atual incorreta" });
    }

    /// <summary>
    /// Solicita token de reset de senha
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request.Email);
        return Ok(new { success = true, message = "Se o email existir, um link de redefinição será enviado" });
    }

    /// <summary>
    /// Redefine a senha usando token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Token inválido ou expirado" });
    }

    /// <summary>
    /// Verifica o email do usuário
    /// </summary>
    [HttpPost("verify-email")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.VerifyEmailAsync(userId.Value, request.Token);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Token inválido" });
    }

    /// <summary>
    /// Configura 2FA (gera secret + QR code)
    /// </summary>
    [HttpPost("2fa/setup")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> Setup2Fa()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.Setup2FaAsync(userId.Value);
        if (result == null) return NotFound();

        var (secret, qrCodeUrl) = result.Value;
        return Ok(new { success = true, data = new { secret, qrCodeUrl } });
    }

    /// <summary>
    /// Ativa 2FA
    /// </summary>
    [HttpPost("2fa/enable")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> Enable2Fa([FromBody] Enable2FaRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.Enable2FaAsync(userId.Value, request.Code);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Código inválido" });
    }

    /// <summary>
    /// Desativa 2FA
    /// </summary>
    [HttpPost("2fa/disable")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> Disable2Fa([FromBody] Disable2FaRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.Disable2FaAsync(userId.Value, request.Code);
        return result ? Ok(new { success = true }) : BadRequest(new { success = false, message = "Código inválido" });
    }

    /// <summary>
    /// Atualiza o perfil do usuário autenticado
    /// </summary>
    [HttpPut("me")]
    [Authorize(AuthenticationSchemes = "Tenant")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var result = await _authService.UpdateProfileAsync(userId.Value, request.Name, request.Phone, request.AvatarUrl);
        return result ? Ok(new { success = true }) : NotFound();
    }
}