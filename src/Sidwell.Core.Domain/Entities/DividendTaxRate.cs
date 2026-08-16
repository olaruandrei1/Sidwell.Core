namespace Sidwell.Core.Domain.Entities;

public sealed class DividendTaxRate
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; } = null!;
    public decimal RatePercent { get; set; }
    public string? Notes { get; set; }
    public string? SourceUrl { get; set; }
    public DateTimeOffset FetchedAt { get; set; }
}
