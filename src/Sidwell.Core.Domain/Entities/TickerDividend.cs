namespace Sidwell.Core.Domain.Entities;

public sealed class TickerDividend
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? ForwardDividend { get; set; }
    public DateOnly? ExDividendDate { get; set; }
    public string? PayFrequency { get; set; }
    public decimal? HistGrowthCagr { get; set; }
    public string Raw { get; set; } = "{}";
    public string? SourceUrl { get; set; }
    public DateTimeOffset FetchedAt { get; set; }
}
