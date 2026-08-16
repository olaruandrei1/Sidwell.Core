namespace Sidwell.Core.Domain.Entities;

public sealed class WealthAllocation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Institution { get; set; } = null!;
    public string InstitutionType { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RON";
    public decimal? InterestRatePct { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
