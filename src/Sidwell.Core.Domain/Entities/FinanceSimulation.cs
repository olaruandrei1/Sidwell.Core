namespace Sidwell.Core.Domain.Entities;

public sealed class FinanceSimulation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public int HorizonYear { get; set; }
    public string BaseCurrency { get; set; } = "RON";
    public string Config { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
