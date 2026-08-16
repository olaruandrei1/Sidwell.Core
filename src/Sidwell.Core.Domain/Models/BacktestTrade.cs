using Sidwell.Core.Domain.Enums;

namespace Sidwell.Core.Domain.Models;

public sealed record BacktestTrade(
    TradeDirection Direction,
    DateOnly EntryDate,
    double EntryPrice,
    DateOnly ExitDate,
    double ExitPrice,
    double ProfitAbsolute,
    double ProfitPercent,
    MarketCondition MarketCondition
);
