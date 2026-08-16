using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sidwell.Core.Infrastructure.Data;
using Sidwell.Core.Infrastructure.Indicators;
using Sidwell.Core.Infrastructure.Recalc;
using Sidwell.Core.Infrastructure.Verdict;

namespace Sidwell.Core.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSidwellInfrastructure(this IServiceCollection services, string connectionString)
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddDbContext<SidwellDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention()
                   .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IRecalcService, NativeRecalcService>();
        services.AddScoped<IIndicatorService, NativeIndicatorService>();
        services.AddScoped<IVerdictService, NativeVerdictService>();

        return services;
    }

    public static async Task MigrateAndInstallSidwellSchemaAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using IServiceScope scope = services.CreateScope();

        SidwellDbContext db = scope.ServiceProvider.GetRequiredService<SidwellDbContext>();

        ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Sidwell.Core.Infrastructure.Schema");

        await db.Database.MigrateAsync(ct);

        await SqlFunctionInstaller.InstallAsync(db, logger, ct);
    }
}
