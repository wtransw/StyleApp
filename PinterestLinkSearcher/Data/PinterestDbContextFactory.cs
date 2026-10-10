using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PinterestLinkSearcher.Data;

public sealed class PinterestDbContextFactory : IDesignTimeDbContextFactory<PinterestDbContext>
{
    public PinterestDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        var options = new DbContextOptionsBuilder<PinterestDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new PinterestDbContext(options);
    }
}
