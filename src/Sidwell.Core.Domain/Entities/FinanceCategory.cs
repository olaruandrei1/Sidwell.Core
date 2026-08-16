namespace Sidwell.Core.Domain.Entities;

public sealed class FinanceCategory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public bool IsDefault { get; set; }
}
