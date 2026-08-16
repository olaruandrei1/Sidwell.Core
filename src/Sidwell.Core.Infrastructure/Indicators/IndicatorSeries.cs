namespace Sidwell.Core.Infrastructure.Indicators;

public sealed record IndicatorPoint(string Date, IReadOnlyDictionary<string, double> Values);

public sealed record IndicatorSeries(
    string Type,
    IReadOnlyDictionary<string, int> Params,
    IReadOnlyList<IndicatorPoint> Points,
    string? Trend,
    string? Error
);
