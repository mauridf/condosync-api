using System.Security.Claims;

namespace CondoSync.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    (string accessToken, string refreshToken) GenerateTokenPair(Guid userId, Guid tenantId, string role, string name, string email);
    (string accessToken, string refreshToken) GenerateAdminTokenPair(Guid adminId, string role, string name, string email);
}