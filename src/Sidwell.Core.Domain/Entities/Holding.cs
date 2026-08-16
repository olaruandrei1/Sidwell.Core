namespace Sidwell.Core.Domain.Entities;

public sealed class Holding
{
    public Guid UserId { get; set; }
    public Guid TickerId { get; set; }
    public decimal Shares { get; set; }
    public decimal AvgCost { get; set; }
    public decimal RealizedPnl { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string Broker { get; set; } = null!;
}
