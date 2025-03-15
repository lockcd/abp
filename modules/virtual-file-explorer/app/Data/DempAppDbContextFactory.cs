using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DempApp.Data;

public class DempAppDbContextFactory : IDesignTimeDbContextFactory<DempAppDbContext>
{
    public DempAppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<DempAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));

        return new DempAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
