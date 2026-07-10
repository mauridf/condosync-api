using CondoSync.Application.Common.Interfaces;
using CondoSync.Core.Interfaces;
using CondoSync.Infrastructure.Data;
using CondoSync.Infrastructure.Data.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant Provider (Singleton - escopo global)
        services.AddSingleton<ITenantProvider, Tenant.TenantProvider>();
        services.AddSingleton<Tenant.TenantInterceptor>();

        // Configurar DbContexts
        services.AddDbContext<AdminDbContext>(options =>
        {
            var connectionString = DatabaseConfig.GetConnectionString(configuration);
            DatabaseConfig.ConfigureNpgsql(options, connectionString);
        });

        services.AddDbContext<CondoSyncDbContext>((serviceProvider, options) =>
        {
            var connectionString = DatabaseConfig.GetConnectionString(configuration);
            DatabaseConfig.ConfigureNpgsql(options, connectionString);

            // Adicionar interceptor de tenant
            var tenantInterceptor = serviceProvider.GetRequiredService<Tenant.TenantInterceptor>();
            options.AddInterceptors(tenantInterceptor);
        });

        // Registrar DbUp Migrator
        services.AddTransient<DatabaseMigrator>();

        // Registrar serviços de infraestrutura
        services.AddScoped(typeof(IRepository<>), typeof(Repositories.GenericRepository<>));
        services.AddScoped<IUnitOfWork, Data.UnitOfWork>();

        services.AddScoped<Infrastructure.Services.AdminDashboardService>();

        services.AddScoped<IPasswordHasher, CondoSync.Application.Services.PasswordService>();
        services.AddScoped<ITokenService, CondoSync.Application.Services.TokenService>();
        services.AddScoped<CondoSync.Application.Services.AuthService>();

        // Serviços externos
        services.AddSingleton<ICacheService>(sp =>
        {
            var connectionString = configuration["Redis:ConnectionString"]
                ?? throw new InvalidOperationException("Redis:ConnectionString não configurada");
            var logger = sp.GetRequiredService<ILogger<External.Cache.RedisCacheService>>();
            return new External.Cache.RedisCacheService(connectionString, logger);
        });

        services.AddSingleton<IMessageBus>(sp =>
        {
            var host = configuration["RabbitMq:Host"] ?? "localhost";
            var port = int.Parse(configuration["RabbitMq:Port"] ?? "5672");
            var username = configuration["RabbitMq:Username"] ?? "guest";
            var password = configuration["RabbitMq:Password"] ?? "guest";
            var logger = sp.GetRequiredService<ILogger<External.MessageBus.RabbitMqMessageBus>>();
            return new External.MessageBus.RabbitMqMessageBus(host, port, username, password, logger);
        });

        services.AddScoped<IStorageService>(sp =>
        {
            var endpoint = configuration["MinIo:Endpoint"] ?? "localhost:9000";
            var accessKey = configuration["MinIo:AccessKey"] ?? "minioadmin";
            var secretKey = configuration["MinIo:SecretKey"] ?? "minioadmin";
            var bucketName = configuration["MinIo:BucketName"] ?? "condosync";
            var logger = sp.GetRequiredService<ILogger<External.Storage.MinioStorageService>>();
            return new External.Storage.MinioStorageService(endpoint, accessKey, secretKey, bucketName, logger);
        });

        services.AddScoped<INotificationService, External.Notification.NotificationDispatcherService>();

        return services;
    }
}