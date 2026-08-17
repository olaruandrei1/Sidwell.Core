using System.Data;
using System.Text.Json;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sidwell.Core.Infrastructure.Broadcast;
using Sidwell.Core.Infrastructure.Data;

namespace Sidwell.Core.Infrastructure.Recalc;

public sealed class NativeRecalcService(
    SidwellDbContext db,
    IBroadcastPublisher broadcast,
    ILogger<NativeRecalcService> logger
) : IRecalcService
{
    private const decimal PriceDropThresholdPct = 5m;

    private static readonly (string Name, int Id)[] PhilosophyMap =
    [
        ("BALANCED", 0),
        ("MOMENTUM", 1),
        ("MEAN_REVERSION", 2),
        ("FUNDAMENTAL", 3),
    ];

    public async Task<RecalcResult> RecalcTickerAsync(
        Guid tickerId,
        DateOnly asOf,
        decimal? technicalScore = null,
        CancellationToken ct = default)
    {
        List<string> ran = [];
        List<RecalcSkip> skipped = [];

        System.Data.Common.DbConnection conn = db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(ct);
        }

        string symbol = await conn.ExecuteScalarAsync<string>(
            "SELECT symbol FROM tickers WHERE id = @id",
            new { id = tickerId }) ?? "UNKNOWN";

        NativeFundamentalSnapshot[] snapshots = await FetchFundamentalSnapshotsAsync(conn, tickerId, asOf, ct);
        bool hasFundamentals = snapshots.Length >= 1;

        IntPtr engine = SidwellQuantNative.sw_engine_create(1024 * 1024 * 4);
        try
        {
            // Prices must be loaded before scoring — algo_momentum (and others) read
            // engine-resident price series, and sw_score_ticker is safe to call even
            // without fundamentals: each algorithm self-reports SW_ERR_INSUFFICIENT_DATA
            // when its own required inputs are missing (see SaveNativeScoresAsync's
            // per-result Status filter), so price/dividend-only algos like momentum can
            // still run for fundamentals-less tickers (e.g. BVB).
            await LoadPricesToEngineAsync(conn, engine, symbol, tickerId, asOf);

            if (hasFundamentals)
            {
                int loadRes = SidwellQuantNative.sw_load_fundamentals_raw(
                    engine,
                    symbol,
                    snapshots,
                    (nuint)snapshots.Length);

                if (loadRes != 0)
                {
                    skipped.Add(new RecalcSkip("native_scoring", "sw_load_fundamentals_raw failed"));
                }
            }
            else
            {
                skipped.Add(new RecalcSkip("native_scoring", "No fundamentals — fundamentals-dependent algorithms will self-report insufficient data"));
            }

            NativeScoreResult[] scores = new NativeScoreResult[32];
            nuint scoreCount = 32;

            int scoreRes = SidwellQuantNative.sw_score_ticker(engine, symbol, scores, ref scoreCount);
            if (scoreRes == 0 && scoreCount > 0)
            {
                await SaveNativeScoresAsync(conn, tickerId, asOf, scores, (int)scoreCount, ran, ct);
            }
            else
            {
                skipped.Add(new RecalcSkip("native_scoring", "sw_score_ticker returned no results"));
            }

            const string upsertCompositeSql = """
                INSERT INTO algorithm_scores (ticker_id, algorithm_name, score, details, as_of_date, philosophy)
                VALUES (@TickerId, 'composite', @Score, @DetailsJson::jsonb, @AsOfDate, @Philosophy)
                ON CONFLICT (ticker_id, algorithm_name, as_of_date, philosophy)
                DO UPDATE SET
                    score = EXCLUDED.score,
                    details = EXCLUDED.details
                """;

            foreach ((string philName, int philId) in PhilosophyMap)
            {
                double tech;
                if (technicalScore is not null)
                {
                    tech = (double)technicalScore.Value;
                }
                else
                {
                    int techRes = SidwellQuantNative.sw_technical_score(engine, symbol, philId, out tech);
                    if (techRes != 0) tech = 0.0;
                }

                double compositeValue;
                NativeScoreResult compScore = default;
                bool usedTechOnly = false;

                if (hasFundamentals)
                {
                    int compRes = SidwellQuantNative.sw_algo_composite(engine, symbol, tech, out compScore);
                    if (compRes == 0 && compScore.Status == 0)
                    {
                        compositeValue = compScore.Score;
                    }
                    else
                    {
                        // Composite failed even with fundamentals — fall back to technical
                        compositeValue = TechnicalToCompositeScale(tech);
                        usedTechOnly = true;
                    }
                }
                else
                {
                    // No fundamentals (e.g. BVB tickers) — use technical score mapped to 0..10 scale
                    compositeValue = TechnicalToCompositeScale(tech);
                    usedTechOnly = true;
                }

                string compDetailsJson = usedTechOnly
                    ? BuildTechnicalOnlyDetails(compositeValue, tech)
                    : BuildCompositeDetails(compScore);

                await conn.ExecuteAsync(upsertCompositeSql, new
                {
                    TickerId = tickerId,
                    Score = Safe(compositeValue),
                    DetailsJson = compDetailsJson,
                    AsOfDate = asOf,
                    Philosophy = philName
                });
                ran.Add($"native_composite:{philName}{(usedTechOnly ? "(tech-only)" : "")}");
            }

            logger.LogInformation("NativeRecalc {Ticker} @ {AsOf}: {RanCount} ran, {SkipCount} skipped",
                tickerId, asOf, ran.Count, skipped.Count);

            await MaybeEmitPriceDropAsync(conn, tickerId, asOf, ct);
        }
        finally
        {
            SidwellQuantNative.sw_engine_destroy(engine);
        }

        return new RecalcResult(tickerId, asOf, ran, skipped);
    }

    private static double Safe(double v) => double.IsFinite(v) ? Math.Round(v, 4) : 0.0;

    /// Maps technical score from [-100, 100] to composite [0, 10].
    private static double TechnicalToCompositeScale(double tech)
    {
        if (!double.IsFinite(tech)) return 5.0;
        double clamped = Math.Max(-100.0, Math.Min(100.0, tech));
        return Math.Round((clamped + 100.0) / 20.0, 4); // -100→0, 0→5, +100→10
    }

    private static string BuildTechnicalOnlyDetails(double compositeValue, double techRaw)
    {
        string label = compositeValue switch
        {
            >= 8.0 => "Strong Technical",
            >= 6.5 => "Positive Technical",
            >= 5.0 => "Neutral Technical",
            >= 3.5 => "Weak Technical",
            _ => "Bearish Technical"
        };
        string color = compositeValue switch
        {
            >= 8.0 => "#10B981",
            >= 6.5 => "#34D399",
            >= 5.0 => "#EAB308",
            >= 3.5 => "#F59E0B",
            _ => "#EF4444"
        };
        var obj = new
        {
            outputs = new
            {
                label,
                color,
                overridden = false,
                confidence = 0.5,
                interpretation = "Technical-only score (no fundamentals available for this ticker)",
                rawValue = Safe(techRaw),
                technicalOnly = true
            }
        };
        return JsonSerializer.Serialize(obj);
    }

    private static string BuildCompositeDetails(NativeScoreResult r)
    {
        string label = r.Score switch
        {
            >= 8.0 => "Strong Conviction",
            >= 6.5 => "Positive Lean",
            >= 5.0 => "Mild-Favorable",
            >= 3.5 => "Mix-Feelings",
            _ => "Weak / Avoid"
        };
        string color = r.Score switch
        {
            >= 8.0 => "#10B981",
            >= 6.5 => "#34D399",
            >= 5.0 => "#EAB308",
            >= 3.5 => "#F59E0B",
            _ => "#EF4444"
        };
        var obj = new
        {
            outputs = new
            {
                label,
                color,
                overridden = false,
                confidence = Safe(r.Confidence),
                interpretation = r.GetInterpretation(),
                rawValue = Safe(r.RawValue)
            }
        };
        return JsonSerializer.Serialize(obj);
    }

    private static string BuildAlgoDetails(NativeScoreResult r)
    {
        var obj = new
        {
            outputs = new
            {
                rawValue = Safe(r.RawValue),
                confidence = Safe(r.Confidence),
                interpretation = r.GetInterpretation()
            }
        };
        return JsonSerializer.Serialize(obj);
    }

    private static string GetAlgoName(int algoId) => algoId switch
    {
        0 => "piotroski",
        1 => "altman_z",
        2 => "greenblatt",
        3 => "dcf",
        4 => "pe_projections",
        5 => "peg",
        6 => "ddm",
        7 => "momentum",
        8 => "accruals",
        9 => "gross_profitability",
        10 => "beneish_m",
        11 => "acquirers",
        12 => "montier_c",
        13 => "mohanram_g",
        14 => "composite",
        _ => "unknown"
    };

    private static async Task LoadPricesToEngineAsync(
        IDbConnection conn,
        IntPtr engine,
        string symbol,
        Guid tickerId,
        DateOnly asOf)
    {
        const string sql = """
            SELECT * FROM (
                SELECT
                    EXTRACT(EPOCH FROM date::timestamp)::double precision AS date,
                    open::double precision AS open,
                    high::double precision AS high,
                    low::double precision AS low,
                    close::double precision AS close,
                    volume::double precision AS volume
                FROM price_history
                WHERE ticker_id = @tickerId AND date <= @asOf
                ORDER BY date DESC
                LIMIT 300
            ) sub
            ORDER BY date ASC
            """;
        List<double> flat = [];
        IEnumerable<dynamic> rows = await conn.QueryAsync(sql, new { tickerId, asOf });
        foreach (dynamic r in rows)
        {
            flat.Add((double)r.date);
            flat.Add((double)r.open);
            flat.Add((double)r.high);
            flat.Add((double)r.low);
            flat.Add((double)r.close);
            flat.Add((double)r.volume);
        }
        if (flat.Count > 0)
        {
            SidwellQuantNative.sw_load_prices(engine, symbol, [.. flat], (nuint)(flat.Count / 6));
        }
    }

    private static async Task<NativeFundamentalSnapshot[]> FetchFundamentalSnapshotsAsync(
        IDbConnection conn,
        Guid tickerId,
        DateOnly asOf,
        CancellationToken ct)
    {
        const string sql = """
            SELECT
                EXTRACT(YEAR FROM f.as_of_date)::int AS fiscal_year,
                COALESCE(f.revenue, 0) AS revenue,
                COALESCE(f.net_income, 0) AS net_income,
                COALESCE(f.operating_cash_flow, 0) AS operating_cash_flow,
                COALESCE(f.total_assets, 0) AS total_assets,
                COALESCE(f.current_assets, 0) AS current_assets,
                COALESCE(f.current_liabilities, 0) AS current_liabilities,
                COALESCE(f.long_term_debt, 0) AS long_term_debt,
                COALESCE(f.shares_outstanding, 0) AS shares_outstanding,
                COALESCE((SELECT ph.close FROM price_history ph
                          WHERE ph.ticker_id = f.ticker_id AND ph.date <= f.as_of_date
                          ORDER BY ph.date DESC LIMIT 1), 0) AS price,
                COALESCE(f.capex, 0) AS capex,
                COALESCE(f.accounts_receivable, 0) AS receivables,
                COALESCE(f.gross_profit, 0) AS gross_profit,
                COALESCE(f.ppe_net, 0) AS pp_and_e,
                COALESCE(f.depreciation, 0) AS depreciation,
                COALESCE(f.sga_expense, 0) AS sga_expense,
                COALESCE(f.ebit, 0) AS ebit,
                COALESCE(f.ebitda, 0) AS ebitda,
                COALESCE(f.eps, 0) AS eps,
                COALESCE(f.total_liabilities, 0) AS total_liabilities,
                COALESCE(f.retained_earnings, 0) AS retained_earnings,
                COALESCE(f.total_equity, 0) AS total_equity,
                COALESCE(f.free_cash_flow, 0) AS free_cash_flow,
                COALESCE(f.market_cap, 0) AS market_cap,
                COALESCE(f.dividend_per_share, 0) AS dividend_per_share
            FROM fundamentals f
            WHERE f.ticker_id = @tickerId AND f.as_of_date <= @asOf
            ORDER BY f.as_of_date DESC
            LIMIT 5
            """;

        List<dynamic> rows = (await conn.QueryAsync(sql, new { tickerId, asOf })).ToList();
        NativeFundamentalSnapshot[] result = new NativeFundamentalSnapshot[rows.Count];

        for (int i = 0; i < rows.Count; i++)
        {
            dynamic r = rows[i];
            result[i] = new NativeFundamentalSnapshot
            {
                FiscalYear = (int)r.fiscal_year,
                Revenue = Convert.ToDouble(r.revenue),
                NetIncome = Convert.ToDouble(r.net_income),
                OperatingCashFlow = Convert.ToDouble(r.operating_cash_flow),
                TotalAssets = Convert.ToDouble(r.total_assets),
                CurrentAssets = Convert.ToDouble(r.current_assets),
                CurrentLiabilities = Convert.ToDouble(r.current_liabilities),
                LongTermDebt = Convert.ToDouble(r.long_term_debt),
                SharesOutstanding = Convert.ToDouble(r.shares_outstanding),
                Price = Convert.ToDouble(r.price),
                Capex = Convert.ToDouble(r.capex),
                Receivables = Convert.ToDouble(r.receivables),
                GrossProfit = Convert.ToDouble(r.gross_profit),
                PpAndE = Convert.ToDouble(r.pp_and_e),
                Depreciation = Convert.ToDouble(r.depreciation),
                SgaExpense = Convert.ToDouble(r.sga_expense),
                Ebit = Convert.ToDouble(r.ebit),
                Ebitda = Convert.ToDouble(r.ebitda),
                Eps = Convert.ToDouble(r.eps),
                TotalLiabilities = Convert.ToDouble(r.total_liabilities),
                RetainedEarnings = Convert.ToDouble(r.retained_earnings),
                TotalEquity = Convert.ToDouble(r.total_equity),
                FreeCashFlow = Convert.ToDouble(r.free_cash_flow),
                MarketCap = Convert.ToDouble(r.market_cap),
                DividendPerShare = Convert.ToDouble(r.dividend_per_share)
            };
        }

        return result;
    }

    private static async Task SaveNativeScoresAsync(
        IDbConnection conn,
        Guid tickerId,
        DateOnly asOf,
        NativeScoreResult[] scores,
        int count,
        List<string> ran,
        CancellationToken ct)
    {
        const string upsertSql = """
            INSERT INTO algorithm_scores (ticker_id, algorithm_name, score, details, as_of_date, philosophy)
            VALUES (@TickerId, @AlgoName, @Score, @DetailsJson::jsonb, @AsOfDate, 'ALL')
            ON CONFLICT (ticker_id, algorithm_name, as_of_date, philosophy)
            DO UPDATE SET
                score = EXCLUDED.score,
                details = EXCLUDED.details
            """;

        for (int i = 0; i < count; i++)
        {
            NativeScoreResult s = scores[i];
            if (s.Status != 0) continue;

            string detailsJson = BuildAlgoDetails(s);
            await conn.ExecuteAsync(upsertSql, new
            {
                TickerId = tickerId,
                AlgoName = GetAlgoName(s.AlgoId),
                Score = Safe(s.Score),
                DetailsJson = detailsJson,
                AsOfDate = asOf
            });

            ran.Add($"native_algo_{GetAlgoName(s.AlgoId)}");
        }
    }

    private async Task MaybeEmitPriceDropAsync(IDbConnection conn, Guid tickerId, DateOnly asOf, CancellationToken ct)
    {
        try
        {
            List<decimal> closes = (await conn.QueryAsync<decimal>(new CommandDefinition(
                "SELECT close FROM price_history WHERE ticker_id = @t AND date <= @d ORDER BY date DESC LIMIT 2",
                new { t = tickerId, d = asOf }, cancellationToken: ct))).ToList();

            if (closes.Count < 2 || closes[1] <= 0m)
                return;

            decimal latest = closes[0];
            decimal previous = closes[1];
            decimal changePct = (latest - previous) / previous * 100m;

            if (changePct > -PriceDropThresholdPct)
                return;

            string? symbol = await conn.ExecuteScalarAsync<string>(new CommandDefinition(
                "SELECT symbol FROM tickers WHERE id = @t", new { t = tickerId }, cancellationToken: ct));

            List<Guid> userIds = (await conn.QueryAsync<Guid>(new CommandDefinition(
                """
                SELECT user_id FROM holdings WHERE ticker_id = @t AND shares > 0
                UNION
                SELECT user_id FROM watchlist WHERE ticker_id = @t
                """, new { t = tickerId }, cancellationToken: ct))).ToList();

            if (userIds.Count == 0)
                return;

            object payload = new
            {
                symbol,
                tickerId = tickerId.ToString(),
                latestClose = latest,
                previousClose = previous,
                changePct = Math.Round(changePct, 2),
                asOf = asOf.ToString("yyyy-MM-dd"),
            };

            foreach (Guid userId in userIds)
                await broadcast.PublishAsync("PRICE_DROP_ALERT", userId, payload, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Price-drop alert check failed for {Ticker} (non-fatal)", tickerId);
        }
    }
}
