using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CondoSync.Infrastructure.Data;

public static class DatabaseConfig
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        return configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Connection string não configurada");
    }

    public static void ConfigureNpgsql(DbContextOptionsBuilder optionsBuilder, string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);

            npgsqlOptions.CommandTimeout(30);

            // Configurar timestamp com timezone como padrão
            npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });

        // Habilitar logging detalhado em desenvolvimento
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
    }
}