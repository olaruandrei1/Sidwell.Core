namespace Sidwell.Core.Domain.Entities;

public sealed class ExchangeRate
{
    public string Currency { get; set; } = null!;
    public DateOnly RateDate { get; set; }
    public decimal RateToRon { get; set; }
    public string Source { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
