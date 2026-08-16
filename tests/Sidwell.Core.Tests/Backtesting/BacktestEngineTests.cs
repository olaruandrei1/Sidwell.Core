using Sidwell.Core.Analytics.Backtesting;
using Sidwell.Core.Domain.Enums;
using Sidwell.Core.Domain.Models;

namespace Sidwell.Core.Tests.Backtesting;

public class BacktestEngineTests
{
    private static DateOnly D(int day)
    {
        return new DateOnly(2024, 1, day);
    }

    [Fact]
    public void Run_SmaCrossover_FullTradeCycle_MatchesHandComputedMetrics()
    {
        Candle[] candles =
        [
            new Candle(D(1), Close: 100, Ema9: 9, Ema50: 10),
            new Candle(D(2), Close: 110, Ema9: 11, Ema50: 10),
            new Candle(D(3), Close: 121, Ema9: 12, Ema50: 10),
            new Candle(D(4), Close: 108.9, Ema9: 9, Ema50: 10),
        ];

        BacktestResult result = BacktestEngine.Run(candles, StrategyId.SmaCrossover, initialCapital: 1000);

        Assert.Equal(990.00, result.FinalCapital, precision: 2);
        Assert.Equal(-10.00, result.TotalReturn, precision: 2);
        Assert.Equal(-1.00, result.TotalReturnPercent, precision: 2);
        Assert.Equal(89.00, result.BenchmarkReturn, precision: 2);
        Assert.Equal(8.90, result.BenchmarkReturnPercent, precision: 2);
        Assert.Equal(0.00, result.WinRate, precision: 2);
        Assert.Equal(1, result.TotalTrades);
        Assert.Equal(0, result.Wins);
        Assert.Equal(1, result.Losses);
        Assert.Equal(0.00, result.ProfitFactor, precision: 2);
        Assert.Equal(10.00, result.MaxDrawdownPercent, precision: 2);
        Assert.Equal(0.00, result.SharpeRatio, precision: 2);

        BacktestTrade trade = Assert.Single(result.Trades);
        Assert.Equal(TradeDirection.Long, trade.Direction);
        Assert.Equal(D(2), trade.EntryDate);
        Assert.Equal(110, trade.EntryPrice);
        Assert.Equal(D(4), trade.ExitDate);
        Assert.Equal(108.9, trade.ExitPrice, precision: 6);
        Assert.Equal(-10.00, trade.ProfitAbsolute, precision: 2);
        Assert.Equal(-1.00, trade.ProfitPercent, precision: 2);
        Assert.Equal(MarketCondition.Ranging, trade.MarketCondition);

        Assert.Equal(4, result.EquityCurve.Count);
        Assert.Equal(1000.00, result.EquityCurve[0].StrategyValue, precision: 2);
        Assert.Equal(1000.00, result.EquityCurve[1].StrategyValue, precision: 2);
        Assert.Equal(1100.00, result.EquityCurve[2].StrategyValue, precision: 2);
        Assert.Equal(990.00, result.EquityCurve[3].StrategyValue, precision: 2);
    }

    [Fact]
    public void Run_PositionStillOpenAtEnd_ForcesCloseOnLastCandle()
    {
        Candle[] candles =
        [
            new Candle(D(1), Close: 100, Ema9: 11, Ema50: 10),
            new Candle(D(2), Close: 110, Ema9: 12, Ema50: 10),
        ];

        BacktestResult result = BacktestEngine.Run(candles, StrategyId.SmaCrossover, initialCapital: 1000);

        BacktestTrade trade = Assert.Single(result.Trades);
        Assert.Equal(D(1), trade.EntryDate);
        Assert.Equal(D(2), trade.ExitDate);
        Assert.Equal(100.00, trade.ProfitAbsolute, precision: 2);
        Assert.Equal(10.00, trade.ProfitPercent, precision: 2);
        Assert.Equal(1, result.Wins);
        Assert.Equal(1100.00, result.FinalCapital, precision: 2);
        Assert.Equal(1100.00, result.EquityCurve[^1].StrategyValue, precision: 2);
    }

    [Fact]
    public void Run_RsiOversoldOverbought_BuysBelow30SellsAbove70()
    {
        Candle[] candles =
        [
            new Candle(D(1), Close: 50, Rsi14: 40),
            new Candle(D(2), Close: 48, Rsi14: 25),
            new Candle(D(3), Close: 52, Rsi14: 75),
        ];

        BacktestResult result = BacktestEngine.Run(candles, StrategyId.RsiOversoldOverbought, initialCapital: 1000);

        BacktestTrade trade = Assert.Single(result.Trades);
        Assert.Equal(48, trade.EntryPrice);
        Assert.Equal(52, trade.ExitPrice);
        Assert.Equal(83.33, trade.ProfitAbsolute, precision: 2);
        Assert.Equal(8.33, trade.ProfitPercent, precision: 2);
        Assert.Equal(1, result.Wins);
        Assert.Equal(0, result.Losses);
    }

    [Fact]
    public void Run_MacdMomentum_BuysOnBullishCrossSellsOnBearishCross()
    {
        Candle[] candles =
        [
            new Candle(D(1), Close: 50, MacdLine: -0.5, MacdSignal: 0),
            new Candle(D(2), Close: 52, MacdLine: 0.5, MacdSignal: 0),
            new Candle(D(3), Close: 55, MacdLine: -0.2, MacdSignal: 0),
        ];

        BacktestResult result = BacktestEngine.Run(candles, StrategyId.MacdMomentum, initialCapital: 1000);

        BacktestTrade trade = Assert.Single(result.Trades);
        Assert.Equal(52, trade.EntryPrice);
        Assert.Equal(55, trade.ExitPrice);
        Assert.Equal(57.69, trade.ProfitAbsolute, precision: 2);
        Assert.Equal(5.77, trade.ProfitPercent, precision: 2);
        Assert.Equal(1, result.Wins);
    }

    [Fact]
    public void Run_NoCandles_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            BacktestEngine.Run([], StrategyId.SmaCrossover, initialCapital: 1000));
    }

    [Fact]
    public void Run_NonPositiveInitialCapital_Throws()
    {
        Candle[] candles = [new Candle(D(1), Close: 100)];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BacktestEngine.Run(candles, StrategyId.SmaCrossover, initialCapital: 0));
    }
}
