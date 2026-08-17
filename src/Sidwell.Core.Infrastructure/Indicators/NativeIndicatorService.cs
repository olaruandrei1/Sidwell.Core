using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Sidwell.Core.Infrastructure.Data;
using Sidwell.Core.Infrastructure.Recalc;

namespace Sidwell.Core.Infrastructure.Indicators;

public sealed partial class NativeIndicatorService(SidwellDbContext db) : IIndicatorService
{
    private sealed record PriceBars(string[] Dates, double[] Close, double[] High, double[] Low, double[] Volume);

    public async Task<IReadOnlyList<IndicatorSeries>> ComputeAsync(
        Guid tickerId, IReadOnlyList<string> requestedTypes, CancellationToken ct = default)
    {
        PriceBars bars = await FetchBarsAsync(tickerId, ct);

        if (bars.Close.Length == 0)
            return requestedTypes.Select(t => Fail(t, "No price data available")).ToList();

        IntPtr engine = SidwellQuantNative.sw_engine_create(1024 * 1024);
        try
        {
            return requestedTypes.Select(raw => ComputeOne(engine, raw, bars)).ToList();
        }
        finally
        {
            SidwellQuantNative.sw_engine_destroy(engine);
        }
    }

    private static IndicatorSeries ComputeOne(IntPtr engine, string raw, PriceBars bars)
    {
        (string kind, int period) = ParseType(raw);
        int n = bars.Close.Length;

        switch (kind)
        {
            case "sma":
            {
                if (period <= 0 || n < period) return Fail(raw, "Not enough price history");
                double[] outRes = new double[n - period + 1];
                if (SidwellQuantNative.sw_ind_sma(engine, bars.Close, (nuint)n, period, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("sma", period, bars.Dates, outRes, 1, ["value"], LatestVsPriceTrend(bars, outRes, 1, 0));
            }
            case "ema":
            {
                if (period <= 0 || n < period) return Fail(raw, "Not enough price history");
                double[] outRes = new double[n];
                if (SidwellQuantNative.sw_ind_ema(engine, bars.Close, (nuint)n, period, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("ema", period, bars.Dates, outRes, 1, ["value"], LatestVsPriceTrend(bars, outRes, 1, 0));
            }
            case "rsi":
            {
                if (period <= 0 || n <= period) return Fail(raw, "Not enough price history");
                double[] outRes = new double[n - period];
                if (SidwellQuantNative.sw_ind_rsi(engine, bars.Close, (nuint)n, period, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("rsi", period, bars.Dates, outRes, 1, ["value"], RsiTrend(outRes));
            }
            case "bb":
            {
                if (period <= 0 || n < period) return Fail(raw, "Not enough price history");
                double[] outRes = new double[(n - period + 1) * 4];
                if (SidwellQuantNative.sw_ind_bollinger(engine, bars.Close, (nuint)n, period, 2.0, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("bb", period, bars.Dates, outRes, 4, ["upper", "middle", "lower", "pctB"],
                    BollingerTrend(outRes));
            }
            case "macd":
            {
                const int fast = 12, slow = 26, signal = 9;
                int maxPeriod = Math.Max(fast, slow);
                int outLen = n - maxPeriod - signal + 2;
                if (outLen <= 0) return Fail(raw, "Not enough price history");
                double[] outRes = new double[outLen * 3];
                if (SidwellQuantNative.sw_ind_macd(engine, bars.Close, (nuint)n, fast, slow, signal, outRes) != 0)
                    return Fail(raw, "Computation failed");
                IndicatorSeries series = Build("macd", 0, bars.Dates, outRes, 3, ["line", "signal", "histogram"], MacdTrend(outRes));
                return series with { Params = new Dictionary<string, int> { ["fast"] = fast, ["slow"] = slow, ["signal"] = signal } };
            }
            case "atr":
            {
                if (period <= 0 || n <= period) return Fail(raw, "Not enough price history");
                double[] outRes = new double[n - period];
                if (SidwellQuantNative.sw_ind_atr(engine, bars.High, bars.Low, bars.Close, (nuint)n, period, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("atr", period, bars.Dates, outRes, 1, ["value"], null);
            }
            case "adx":
            {
                if (period <= 0 || n <= period * 2) return Fail(raw, "Not enough price history");
                int calcLen = n - 1;
                int smoothedLen = calcLen - period + 1;
                int outLen = smoothedLen - period + 1;
                if (outLen <= 0) return Fail(raw, "Not enough price history");
                double[] outRes = new double[outLen * 3];
                if (SidwellQuantNative.sw_ind_adx(engine, bars.High, bars.Low, bars.Close, (nuint)n, period, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("adx", period, bars.Dates, outRes, 3, ["adx", "plusDi", "minusDi"], AdxTrend(outRes));
            }
            case "obv":
            {
                double[] outRes = new double[n];
                if (SidwellQuantNative.sw_ind_obv(engine, bars.Close, bars.Volume, (nuint)n, outRes) != 0)
                    return Fail(raw, "Computation failed");
                return Build("obv", 0, bars.Dates, outRes, 1, ["value"], null);
            }
            default:
                return Fail(raw, $"Unknown indicator type '{kind}'");
        }
    }

    private static IndicatorSeries Build(
        string type, int period, string[] dates, double[] flat, int valuesPerPoint, string[] valueNames, string? trend)
    {
        int outLen = flat.Length / valuesPerPoint;
        int dateOffset = dates.Length - outLen;
        List<IndicatorPoint> points = new(outLen);

        for (int i = 0; i < outLen; i++)
        {
            Dictionary<string, double> values = new();
            bool anyNaN = false;
            for (int v = 0; v < valuesPerPoint; v++)
            {
                double val = flat[i * valuesPerPoint + v];
                if (double.IsNaN(val) || double.IsInfinity(val)) { anyNaN = true; break; }
                values[valueNames[v]] = Math.Round(val, 4);
            }
            if (!anyNaN && dateOffset + i >= 0)
                points.Add(new IndicatorPoint(dates[dateOffset + i], values));
        }

        Dictionary<string, int> paramDict = period > 0 ? new() { ["period"] = period } : new();
        return new IndicatorSeries(type, paramDict, points, trend, null);
    }

    private static IndicatorSeries Fail(string raw, string reason)
    {
        (string kind, int period) = ParseType(raw);
        Dictionary<string, int> paramDict = period > 0 ? new() { ["period"] = period } : new();
        return new IndicatorSeries(kind, paramDict, [], null, reason);
    }

    private static string? LatestVsPriceTrend(PriceBars bars, double[] outRes, int valuesPerPoint, int valueIndex)
    {
        if (outRes.Length < valuesPerPoint) return null;
        double latestIndicator = outRes[^(valuesPerPoint - valueIndex)];
        double latestClose = bars.Close[^1];
        if (double.IsNaN(latestIndicator)) return null;
        if (latestClose > latestIndicator) return "above";
        if (latestClose < latestIndicator) return "below";
        return "at";
    }

    private static string? RsiTrend(double[] outRes)
    {
        if (outRes.Length == 0) return null;
        double latest = outRes[^1];
        if (latest >= 70) return "overbought";
        if (latest <= 30) return "oversold";
        return "neutral";
    }

    private static string? BollingerTrend(double[] outRes)
    {
        if (outRes.Length < 4) return null;
        double pctB = outRes[^1];
        if (pctB >= 1.0) return "above-upper-band";
        if (pctB <= 0.0) return "below-lower-band";
        return "within-bands";
    }

    private static string? MacdTrend(double[] outRes)
    {
        if (outRes.Length < 3) return null;
        double line = outRes[^3];
        double signal = outRes[^2];
        return line > signal ? "bullish-crossover" : "bearish-crossover";
    }

    private static string? AdxTrend(double[] outRes)
    {
        if (outRes.Length < 3) return null;
        double adx = outRes[^3];
        return adx >= 25 ? "strong-trend" : "weak-trend";
    }

    [GeneratedRegex(@"^([a-z]+?)(\d*)$")]
    private static partial Regex TypeRegex();

    private static (string Kind, int Period) ParseType(string raw)
    {
        Match match = TypeRegex().Match(raw.Trim().ToLowerInvariant());
        if (!match.Success) return (raw, 0);

        string kind = match.Groups[1].Value;
        string periodStr = match.Groups[2].Value;

        int defaultPeriod = kind switch
        {
            "sma" or "ema" or "bb" => 20,
            "rsi" or "atr" or "adx" => 14,
            _ => 0
        };

        int period = periodStr.Length > 0 ? int.Parse(periodStr) : defaultPeriod;
        return (kind, period);
    }

    private async Task<PriceBars> FetchBarsAsync(Guid tickerId, CancellationToken ct)
    {
        const string sql = """
            SELECT * FROM (
                SELECT
                    date::text AS date,
                    close::double precision AS close,
                    high::double precision AS high,
                    low::double precision AS low,
                    volume::double precision AS volume
                FROM price_history
                WHERE ticker_id = @tickerId
                ORDER BY date DESC
                LIMIT 800
            ) sub
            ORDER BY date ASC
            """;

        IDbConnection conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await ((System.Data.Common.DbConnection)conn).OpenAsync(ct);

        List<dynamic> rows = (await conn.QueryAsync(new CommandDefinition(sql, new { tickerId }, cancellationToken: ct))).ToList();

        string[] dates = new string[rows.Count];
        double[] close = new double[rows.Count];
        double[] high = new double[rows.Count];
        double[] low = new double[rows.Count];
        double[] volume = new double[rows.Count];

        for (int i = 0; i < rows.Count; i++)
        {
            dynamic r = rows[i];
            dates[i] = (string)r.date;
            close[i] = (double)r.close;
            high[i] = (double)r.high;
            low[i] = (double)r.low;
            volume[i] = (double)r.volume;
        }

        return new PriceBars(dates, close, high, low, volume);
    }
}
