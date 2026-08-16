namespace Sidwell.Core.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TickerId { get; set; }
    public string Side { get; set; } = null!;
    public decimal Shares { get; set; }
    public decimal Price { get; set; }
    public decimal Fee { get; set; }
    public bool PriceAuto { get; set; }
    public decimal? FxRateAtExecution { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Broker { get; set; } = null!;
}
