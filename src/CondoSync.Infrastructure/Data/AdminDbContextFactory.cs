using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CondoSync.Infrastructure.Data;

public class AdminDbContextFactory : IDesignTimeDbContextFactory<AdminDbContext>
{
    public AdminDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "CondoSync.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = DatabaseConfig.GetConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<AdminDbContext>();
        DatabaseConfig.ConfigureNpgsql(optionsBuilder, connectionString);

        return new AdminDbContext(optionsBuilder.Options);
    }
}