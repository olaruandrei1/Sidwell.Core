namespace Sidwell.Core.Domain.Entities;

public sealed class InsiderTransaction
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public string Insider { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Shares { get; set; }
    public decimal? Price { get; set; }
    public DateOnly TxDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
