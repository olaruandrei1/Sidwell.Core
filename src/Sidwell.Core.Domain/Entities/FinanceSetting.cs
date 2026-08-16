namespace Sidwell.Core.Domain.Entities;

public sealed class FinanceSetting
{
    public Guid UserId { get; set; }
    public decimal MonthlyIncomeAmount { get; set; }
    public string MonthlyIncomeCurrency { get; set; } = "RON";
    public string Banks { get; set; } = "[]";
    public string Brokers { get; set; } = "[]";
    public DateTimeOffset UpdatedAt { get; set; }
}
