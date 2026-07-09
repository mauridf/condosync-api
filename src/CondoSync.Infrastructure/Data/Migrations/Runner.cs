using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CondoSync.Infrastructure.Data.Migrations;

public class DatabaseMigrator
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseMigrator> _logger;

    public DatabaseMigrator(IConfiguration configuration, ILogger<DatabaseMigrator> logger)
    {
        _connectionString = DatabaseConfig.GetConnectionString(configuration);
        _logger = logger;
    }

    public void RunMigrations()
    {
        _logger.LogInformation("🔍 Verificando conexão com o banco de dados...");

        // Garantir que o banco existe
        EnsureDatabase.For.PostgresqlDatabase(_connectionString);

        _logger.LogInformation("📦 Iniciando migrations...");

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptName => scriptName.StartsWith("CondoSync.Infrastructure.Data.Migrations.Scripts.V"))
            .WithTransaction()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            _logger.LogError(result.Error, "❌ Erro ao executar migrations");
            throw result.Error;
        }

        _logger.LogInformation("✅ Migrations executadas com sucesso!");

        // Log das migrations aplicadas
        foreach (var script in result.Scripts)
        {
            _logger.LogInformation("  ✓ {ScriptName}", script.Name);
        }
    }
}