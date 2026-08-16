namespace Sidwell.Core.Domain.Entities;

public sealed class AlgorithmScore
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public string AlgorithmName { get; set; } = null!;
    public string Philosophy { get; set; } = null!;
    public DateOnly AsOfDate { get; set; }
    public decimal? Score { get; set; }
    public string Details { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
}
