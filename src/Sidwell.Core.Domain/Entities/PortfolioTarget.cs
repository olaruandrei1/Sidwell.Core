namespace Sidwell.Core.Domain.Entities;

public sealed class PortfolioTarget
{
    public Guid UserId { get; set; }
    public Guid TickerId { get; set; }
    public decimal TargetShares { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
