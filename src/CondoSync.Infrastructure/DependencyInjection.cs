using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CondoSync.Infrastructure.Data;
using CondoSync.Infrastructure.Data.Migrations;
using CondoSync.Core.Interfaces;

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

        // TODO: Registrar serviços externos quando implementados
        // services.AddScoped<IStorageService, MinioStorageService>();
        // services.AddScoped<ICacheService, RedisCacheService>();
        // services.AddScoped<IMessageBus, RabbitMqMessageBus>();
        // services.AddScoped<IPaymentGateway, PaymentGatewayService>();
        // services.AddScoped<INotificationService, NotificationDispatcherService>();

        return services;
    }
}