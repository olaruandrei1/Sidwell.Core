using Sidwell.Core.Analytics.Extensions;
using Sidwell.Core.Domain.Enums;
using Sidwell.Core.Domain.Models;

namespace Sidwell.Core.Analytics.Backtesting;

public static class BacktestEngine
{
    public static BacktestResult Run(IReadOnlyList<Candle> candles, StrategyId strategy, double initialCapital)
    {
        if (candles.Count == 0)
        {
            throw new ArgumentException("no historical data found for symbol and timeframe", nameof(candles));
        }

        if (initialCapital <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapital), "initialCapital must be > 0");
        }

        double currentCapital = initialCapital;
        double entryPrice = 0;

        List<BacktestTrade> rawTrades = [];
        
        bool inPosition = false;
        
        DateOnly entryDate = default;
        
        MarketCondition entryMarketCondition = MarketCondition.Ranging;
        
        double shares = 0;

        EquityPoint[] equityCurve = new EquityPoint[candles.Count];
        
        double peak = initialCapital;
        double maxDrawdownPercent = 0.0;

        double firstClosePrice = candles[0].Close;

        if (firstClosePrice <= 0)
        {
            firstClosePrice = candles.FirstOrDefault(candidate => candidate.Close > 0)?.Close ?? 1.0;
        }

        double benchmarkShares = initialCapital / firstClosePrice;

        for (int i = 0; i < candles.Count; i++)
        {
            Candle candle = candles[i];

            if (!inPosition)
            {
                bool buyTriggered = candle.TriggersEntry(strategy);

                if (buyTriggered && candle.Close > 0)
                {
                    inPosition = true;
                    entryPrice = candle.Close;
                    entryDate = candle.Date;
                    shares = currentCapital / entryPrice;
                    entryMarketCondition = candle.ResolveEntryMarketCondition();
                }
            }
            else
            {
                bool sellTriggered = candle.TriggersExit(strategy);

                if (sellTriggered && candle.Close > 0)
                {
                    double profitAbsolute = (candle.Close - entryPrice) * shares;
                    double profitPercent = ((candle.Close / entryPrice) - 1.0) * 100.0;
                    currentCapital += profitAbsolute;

                    rawTrades.Add(new BacktestTrade(
                        TradeDirection.Long,
                        entryDate,
                        entryPrice,
                        candle.Date,
                        candle.Close,
                        profitAbsolute.Round2(),
                        profitPercent.Round2(),
                        entryMarketCondition)
                    );

                    inPosition = false;
                }
            }

            double strategyValue = inPosition ? shares * candle.Close : currentCapital;
            double benchmarkValue = benchmarkShares * candle.Close;

            equityCurve[i] = new EquityPoint(candle.Date, strategyValue.Round2(), benchmarkValue.Round2());

            if (strategyValue > peak)
            {
                peak = strategyValue;
            }

            double drawdown = peak > 0 ? (peak - strategyValue) / peak * 100.0 : 0.0;
            
            if (drawdown > maxDrawdownPercent)
            {
                maxDrawdownPercent = drawdown;
            }
        }

        if (inPosition)
        {
            Candle lastCandle = candles[^1];

            double profitAbsolute = (lastCandle.Close - entryPrice) * shares;
            double profitPercent = ((lastCandle.Close / entryPrice) - 1.0) * 100.0;

            currentCapital += profitAbsolute;

            rawTrades.Add(new BacktestTrade(
                TradeDirection.Long,
                entryDate,
                entryPrice,
                lastCandle.Date,
                lastCandle.Close,
                profitAbsolute.Round2(),
                profitPercent.Round2(),
                entryMarketCondition)
            );

            equityCurve[^1] = equityCurve[^1] with { StrategyValue = currentCapital.Round2() };
        }

        double totalReturn = currentCapital - initialCapital;
        double totalReturnPercent = ((currentCapital / initialCapital) - 1.0) * 100.0;

        double benchmarkFinal = benchmarkShares * candles[^1].Close;
        double benchmarkReturn = benchmarkFinal - initialCapital;
        double benchmarkReturnPercent = ((benchmarkFinal / initialCapital) - 1.0) * 100.0;

        int wins = 0;
        int losses = 0;

        double grossProfit = 0;
        double grossLoss = 0;

        foreach (BacktestTrade trade in rawTrades)
        {
            if (trade.ProfitAbsolute > 0)
            {
                wins++;
                grossProfit += trade.ProfitAbsolute;
            }
            else if (trade.ProfitAbsolute < 0)
            {
                losses++;
                grossLoss += Math.Abs(trade.ProfitAbsolute);
            }
        }

        int totalTrades = rawTrades.Count;
        double winRate = totalTrades > 0 ? (double)wins / totalTrades * 100.0 : 0.0;

        double profitFactor;
        if (grossLoss > 0)
        {
            profitFactor = grossProfit / grossLoss;
        }
        else if (grossProfit > 0)
        {
            profitFactor = double.PositiveInfinity;
        }
        else
        {
            profitFactor = 0.0;
        }

        double sharpeRatio = CalculateSharpeRatio(equityCurve);

        return new BacktestResult(
            totalReturn.Round2(),
            totalReturnPercent.Round2(),
            currentCapital.Round2(),
            benchmarkReturn.Round2(),
            benchmarkReturnPercent.Round2(),
            winRate.Round2(),
            totalTrades,
            wins,
            losses,
            profitFactor.Round2(),
            maxDrawdownPercent.Round2(),
            sharpeRatio.Round2(),
            equityCurve,
            rawTrades
        );
    }

    private static double CalculateSharpeRatio(EquityPoint[] equityCurve)
    {
        List<double> dailyReturns = new(Math.Max(0, equityCurve.Length - 1));

        for (int i = 1; i < equityCurve.Length; i++)
        {
            double previous = equityCurve[i - 1].StrategyValue;
            if (previous > 0)
            {
                dailyReturns.Add((equityCurve[i].StrategyValue / previous) - 1.0);
            }
        }

        if (dailyReturns.Count <= 1)
        {
            return 0.0;
        }

        double mean = dailyReturns.Average();

        double sumSquaredDifferences = dailyReturns.Sum(returnValue => (returnValue - mean) * (returnValue - mean));
        double variance = sumSquaredDifferences / (dailyReturns.Count - 1);
        double standardDeviation = Math.Sqrt(variance);

        return standardDeviation > 0 ? Math.Sqrt(252) * (mean / standardDeviation) : 0.0;
    }
}
