namespace Sidwell.Core.Domain.Entities;

public sealed class BrokerFeeSchedule
{
    public Guid Id { get; set; }
    public string Broker { get; set; } = null!;
    public string Market { get; set; } = null!;
    public decimal? Percent { get; set; }
    public decimal? MinFee { get; set; }
    public decimal? FixedFee { get; set; }
    public decimal? FxConversionPercent { get; set; }
    public string? Currency { get; set; }
    public string Raw { get; set; } = "{}";
    public string? SourceUrl { get; set; }
    public DateTimeOffset FetchedAt { get; set; }
}
