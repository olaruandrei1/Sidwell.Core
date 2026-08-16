using Sidwell.Core.Domain.Enums;
using Sidwell.Core.Domain.Models;

namespace Sidwell.Core.Analytics.Backtesting;

public static class CandleExtensions
{
    public static bool TriggersEntry(this Candle candle, StrategyId strategy)
    {
        return strategy switch
        {
            StrategyId.SmaCrossover => candle.HasEma && candle.Ema9 > candle.Ema50,
            StrategyId.RsiOversoldOverbought => candle.HasRsi && candle.Rsi14 < 30,
            StrategyId.MacdMomentum => candle.HasMacd && candle.MacdLine > candle.MacdSignal,
            _ => false,
        };
    }

    public static bool TriggersExit(this Candle candle, StrategyId strategy)
    {
        return strategy switch
        {
            StrategyId.SmaCrossover => candle.HasEma && candle.Ema9 < candle.Ema50,
            StrategyId.RsiOversoldOverbought => candle.HasRsi && candle.Rsi14 > 70,
            StrategyId.MacdMomentum => candle.HasMacd && candle.MacdLine < candle.MacdSignal,
            _ => false,
        };
    }

    public static MarketCondition ResolveEntryMarketCondition(this Candle candle)
    {
        return candle.HasAdx && candle.Adx14 > 25 ? MarketCondition.Trending : MarketCondition.Ranging;
    }
}
