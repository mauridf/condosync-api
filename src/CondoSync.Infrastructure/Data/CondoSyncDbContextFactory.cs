using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CondoSync.Infrastructure.Data;

public class CondoSyncDbContextFactory : IDesignTimeDbContextFactory<CondoSyncDbContext>
{
    public CondoSyncDbContext CreateDbContext(string[] args)
    {
        // Carregar configuração do appsettings.json da API
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CondoSync.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = DatabaseConfig.GetConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<CondoSyncDbContext>();
        DatabaseConfig.ConfigureNpgsql(optionsBuilder, connectionString);

        return new CondoSyncDbContext(optionsBuilder.Options);
    }
}