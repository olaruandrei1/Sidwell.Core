namespace Sidwell.Core.Domain.Models;

public sealed record BacktestResult(
    double TotalReturn,
    double TotalReturnPercent,
    double FinalCapital,
    double BenchmarkReturn,
    double BenchmarkReturnPercent,
    double WinRate,
    int TotalTrades,
    int Wins,
    int Losses,
    double ProfitFactor,
    double MaxDrawdownPercent,
    double SharpeRatio,
    IReadOnlyList<EquityPoint> EquityCurve,
    IReadOnlyList<BacktestTrade> Trades
);
