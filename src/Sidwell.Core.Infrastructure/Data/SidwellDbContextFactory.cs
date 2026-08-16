using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sidwell.Core.Infrastructure.Data;

public sealed class SidwellDbContextFactory : IDesignTimeDbContextFactory<SidwellDbContext>
{
    public SidwellDbContext CreateDbContext(string[] args)
    {
        string connection = Environment.GetEnvironmentVariable("SIDWELL_CORE_DB")
            ?? throw new InvalidOperationException(
                "Set the SIDWELL_CORE_DB environment variable to a valid PostgreSQL connection string.");

        DbContextOptions<SidwellDbContext> options = new DbContextOptionsBuilder<SidwellDbContext>()
            .UseNpgsql(connection)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new SidwellDbContext(options);
    }
}
