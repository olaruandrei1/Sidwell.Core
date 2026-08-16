namespace Sidwell.Core.Domain.Entities;

public sealed class Fundamental
{
    public Guid Id { get; set; }
    public Guid TickerId { get; set; }
    public DateOnly AsOfDate { get; set; }
    public string Period { get; set; } = null!;

    public decimal? Revenue { get; set; }
    public decimal? NetIncome { get; set; }
    public decimal? GrossProfit { get; set; }
    public decimal? Ebit { get; set; }
    public decimal? Ebitda { get; set; }
    public decimal? TotalAssets { get; set; }
    public decimal? TotalLiabilities { get; set; }
    public decimal? TotalEquity { get; set; }
    public decimal? RetainedEarnings { get; set; }
    public decimal? CurrentAssets { get; set; }
    public decimal? CurrentLiabilities { get; set; }
    public decimal? LongTermDebt { get; set; }
    public decimal? TotalDebt { get; set; }
    public decimal? Cash { get; set; }
    public decimal? OperatingCashFlow { get; set; }
    public decimal? Capex { get; set; }
    public decimal? FreeCashFlow { get; set; }
    public decimal? Eps { get; set; }
    public long? SharesOutstanding { get; set; }
    public decimal? DividendPerShare { get; set; }
    public decimal? DividendYield { get; set; }
    public decimal? DividendGrowth { get; set; }
    public decimal? BookValuePerShare { get; set; }
    public decimal? MarketCap { get; set; }
    public decimal? PeRatio { get; set; }
    public decimal? Roe { get; set; }

    public decimal? AccountsReceivable { get; set; }
    public decimal? PpeNet { get; set; }
    public decimal? Depreciation { get; set; }
    public decimal? SgaExpense { get; set; }

    public string Raw { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; }
}
