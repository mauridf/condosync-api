using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CondoSync.Application.Common.Interfaces;

namespace CondoSync.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _adminIssuer;
    private readonly string _adminAudience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;
    private readonly int _adminAccessTokenExpirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;

        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"]!;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        _issuer = jwtSettings["Issuer"]!;
        _audience = jwtSettings["Audience"]!;
        _adminIssuer = jwtSettings["AdminIssuer"]!;
        _adminAudience = jwtSettings["AdminAudience"]!;

        _accessTokenExpirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");
        _refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
        _adminAccessTokenExpirationMinutes = int.Parse(jwtSettings["AdminAccessTokenExpirationMinutes"] ?? "120");
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public (string accessToken, string refreshToken) GenerateTokenPair(
        Guid userId, Guid tenantId, string role, string name, string email)
    {
        var claims = new List<Claim>
        {
            new("userId", userId.ToString()),
            new("tenantId", tenantId.ToString()),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email)
        };

        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        return (accessToken, refreshToken);
    }

    public (string accessToken, string refreshToken) GenerateAdminTokenPair(
        Guid adminId, string role, string name, string email)
    {
        var claims = new List<Claim>
        {
            new("adminId", adminId.ToString()),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_adminAccessTokenExpirationMinutes),
            Issuer = _adminIssuer,
            Audience = _adminAudience,
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        var refreshToken = GenerateRefreshToken();

        return (accessToken, refreshToken);
    }
}