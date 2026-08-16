namespace Sidwell.Core.Domain.Models;

public sealed record Candle(
    DateOnly Date,
    double Close,
    double? Ema9 = null,
    double? Ema50 = null,
    double? Rsi14 = null,
    double? MacdLine = null,
    double? MacdSignal = null,
    double? Adx14 = null,
    double? Atr14 = null
)
{
    public bool HasEma => Ema9.HasValue && Ema50.HasValue;
    public bool HasRsi => Rsi14.HasValue;
    public bool HasMacd => MacdLine.HasValue && MacdSignal.HasValue;
    public bool HasAdx => Adx14.HasValue;
}
