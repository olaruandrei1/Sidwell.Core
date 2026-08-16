namespace Sidwell.Core.Domain.Entities;

public sealed class Expense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Month { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "RON";
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateOnly? DueDate { get; set; }
    public decimal? InterestRatePct { get; set; }
    public bool IsRecurring { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? LineItems { get; set; }
}
