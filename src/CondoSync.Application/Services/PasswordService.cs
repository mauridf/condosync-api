using CondoSync.Application.Common.Interfaces;

namespace CondoSync.Application.Services;

public class PasswordService : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // BCrypt com salt factor 12 (segurança elevada)
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}