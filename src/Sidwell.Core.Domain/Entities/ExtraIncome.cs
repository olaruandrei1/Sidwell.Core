namespace Sidwell.Core.Domain.Entities;

public sealed class ExtraIncome
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Month { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
