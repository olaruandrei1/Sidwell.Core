using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Sidwell.Core.Infrastructure.Data;
using Sidwell.Core.Infrastructure.Indicators;
using Sidwell.Core.Infrastructure.Recalc;

namespace Sidwell.Core.Infrastructure.Verdict;

public sealed class NativeVerdictService(IIndicatorService indicators, SidwellDbContext db) : IVerdictService
{
    private static readonly IReadOnlyDictionary<string, int> KindCodes = new Dictionary<string, int>
    {
        ["sma"] = 0,
        ["ema"] = 1,
        ["bb"] = 2,
        ["rsi"] = 3,
        ["macd"] = 4,
        ["adx"] = 5,
        ["atr"] = 6,
        ["obv"] = 7
    };

    private static readonly IReadOnlyDictionary<int, string> ActionNames = new Dictionary<int, string>
    {
        [0] = "strong_buy",
        [1] = "buy",
        [2] = "hold",
        [3] = "caution",
        [4] = "avoid"
    };

    public async Task<TechnicalVerdictResult> ComputeAsync(
        Guid tickerId, double compositeScore, IReadOnlyList<string> types, CancellationToken ct = default)
    {
        IReadOnlyList<IndicatorSeries> series = await indicators.ComputeAsync(tickerId, types, ct);
        double latestClose = await FetchLatestCloseAsync(tickerId, ct);

        List<NativeIndicatorSignal> signals = [];
        foreach (IndicatorSeries s in series)
        {
            if (s.Error is not null || s.Points.Count == 0) continue;
            if (!KindCodes.TryGetValue(s.Type, out int kindCode)) continue;

            IndicatorPoint last = s.Points[^1];
            double primary = s.Type switch
            {
                "sma" or "ema" or "rsi" or "atr" or "obv" => last.Values.GetValueOrDefault("value"),
                "bb" => last.Values.GetValueOrDefault("pctB"),
                "macd" => last.Values.GetValueOrDefault("histogram"),
                "adx" => last.Values.GetValueOrDefault("adx"),
                _ => 0
            };
            double secondary = s.Type is "sma" or "ema" ? latestClose : 0;

            signals.Add(new NativeIndicatorSignal { Kind = kindCode, Primary = primary, Secondary = secondary });
        }

        IntPtr engine = SidwellQuantNative.sw_engine_create(1024 * 1024);
        try
        {
            int rc = SidwellQuantNative.sw_technical_verdict(
                engine, compositeScore, signals.ToArray(), (nuint)signals.Count, out NativeTechnicalVerdict verdict);

            if (rc != 0)
                return new TechnicalVerdictResult(0, 50, "hold", 0);

            string action = ActionNames.GetValueOrDefault(verdict.Action, "hold");

            ReentryEstimateResult? reentry = null;
            if (action is "caution" or "avoid")
                reentry = await TryComputeReentryAsync(engine, tickerId, ct);

            return new TechnicalVerdictResult(verdict.RawScore, verdict.ConvictionPct, action, verdict.AgreementPct, reentry);
        }
        finally
        {
            SidwellQuantNative.sw_engine_destroy(engine);
        }
    }

    private async Task<ReentryEstimateResult?> TryComputeReentryAsync(IntPtr engine, Guid tickerId, CancellationToken ct)
    {
        double[] close = await FetchCloseHistoryAsync(tickerId, ct);
        const int period = 20;
        if (close.Length < period) return null;

        double[] sma = new double[close.Length - period + 1];
        if (SidwellQuantNative.sw_ind_sma(engine, close, (nuint)close.Length, period, sma) != 0)
            return null;

        if (SidwellQuantNative.sw_estimate_reentry(engine, close, (nuint)close.Length, sma, (nuint)sma.Length, out NativeReentryEstimate estimate) != 0)
            return null;

        if (estimate.Available == 0) return null;

        return new ReentryEstimateResult(estimate.EstimatedDays, estimate.SampleCount, estimate.TargetPrice, estimate.CurrentDeviationPct);
    }

    private async Task<double[]> FetchCloseHistoryAsync(Guid tickerId, CancellationToken ct)
    {
        const string sql = """
            SELECT close FROM (
                SELECT date, close::double precision AS close
                FROM price_history
                WHERE ticker_id = @tickerId
                ORDER BY date DESC
                LIMIT 800
            ) sub
            ORDER BY date ASC;
            """;

        IDbConnection conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await ((System.Data.Common.DbConnection)conn).OpenAsync(ct);

        IEnumerable<double> closes = await conn.QueryAsync<double>(new CommandDefinition(
            sql, new { tickerId }, cancellationToken: ct));

        return closes.ToArray();
    }

    private async Task<double> FetchLatestCloseAsync(Guid tickerId, CancellationToken ct)
    {
        IDbConnection conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await ((System.Data.Common.DbConnection)conn).OpenAsync(ct);

        double? close = await conn.ExecuteScalarAsync<double?>(new CommandDefinition(
            "SELECT close::double precision FROM price_history WHERE ticker_id = @tickerId ORDER BY date DESC LIMIT 1",
            new { tickerId }, cancellationToken: ct));

        return close ?? 0;
    }
}
