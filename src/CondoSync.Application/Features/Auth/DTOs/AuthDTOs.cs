namespace CondoSync.Application.Features.Auth.DTOs;

public record RegisterRequest(
    string CondominiumName,
    string CondominiumSlug,
    string AdminName,
    string AdminEmail,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string Role,
    Guid TenantId,
    string AccessToken,
    string RefreshToken
);

public record AdminLoginRequest(
    string Email,
    string Password
);

public record AdminAuthResponse(
    Guid AdminId,
    string Name,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Token,
    string NewPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);