using CondoSync.Application.Common.Interfaces;
using CondoSync.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CondoSync.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Serviços de autenticação
        services.AddScoped<IPasswordHasher, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<AuthService>();

        // Serviços administrativos
        services.AddScoped<AdminService>();
        services.AddScoped<UnitService>();

        return services;
    }
}