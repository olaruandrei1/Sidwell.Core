namespace Sidwell.Core.Domain.Models;

public sealed record EquityPoint(DateOnly Date, double StrategyValue, double BenchmarkValue);
