using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Sidwell.Core.Infrastructure.Data;

public static class SqlFunctionInstaller
{
    public static async Task InstallAsync(SidwellDbContext db, ILogger logger, CancellationToken ct = default)
    {
        Assembly assembly = typeof(SqlFunctionInstaller).Assembly;
        string[] names = assembly.GetManifestResourceNames();

        DbConnection conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        foreach (string folder in new[] { "algs" })
        {
            string marker = $".SqlFunctions.{folder}.";
            string[] resources = names
                .Where(n => n.Contains(marker, StringComparison.Ordinal) && n.EndsWith(".sql", StringComparison.Ordinal))
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToArray();

            if (resources.Length == 0)
            {
                logger.LogWarning("SqlFunctionInstaller: no embedded scripts found for {Folder}", folder);
                continue;
            }

            foreach (string name in resources)
            {
                await using Stream stream = assembly.GetManifestResourceStream(name)!;
                using StreamReader reader = new StreamReader(stream);
                string sql = await reader.ReadToEndAsync(ct);
                if (string.IsNullOrWhiteSpace(sql))
                    continue;

                await using DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync(ct);
                logger.LogInformation("SqlFunctionInstaller: applied {Resource}", name);
            }
        }
    }
}
