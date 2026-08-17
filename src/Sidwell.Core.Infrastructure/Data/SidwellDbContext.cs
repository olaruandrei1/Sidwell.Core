using Microsoft.EntityFrameworkCore;
using Sidwell.Core.Domain.Entities;

namespace Sidwell.Core.Infrastructure.Data;

public sealed class SidwellDbContext(DbContextOptions<SidwellDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<WebauthnCredential> WebauthnCredentials => Set<WebauthnCredential>();
    public DbSet<Ticker> Tickers => Set<Ticker>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<Fundamental> Fundamentals => Set<Fundamental>();
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();
    public DbSet<SecFiling> SecFilings => Set<SecFiling>();
    public DbSet<InsiderTransaction> InsiderTransactions => Set<InsiderTransaction>();
    public DbSet<AlgorithmScore> AlgorithmScores => Set<AlgorithmScore>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Holding> Holdings => Set<Holding>();
    public DbSet<WatchlistEntry> Watchlist => Set<WatchlistEntry>();
    public DbSet<PortfolioTarget> PortfolioTargets => Set<PortfolioTarget>();
    public DbSet<TickerNote> TickerNotes => Set<TickerNote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();
    public DbSet<ApiCredential> ApiCredentials => Set<ApiCredential>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<TickerDividend> TickerDividends => Set<TickerDividend>();
    public DbSet<BrokerFeeSchedule> BrokerFeeSchedules => Set<BrokerFeeSchedule>();
    public DbSet<DividendTaxRate> DividendTaxRates => Set<DividendTaxRate>();
    public DbSet<FinanceSetting> FinanceSettings => Set<FinanceSetting>();
    public DbSet<FinanceCategory> FinanceCategories => Set<FinanceCategory>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<WealthAllocation> WealthAllocations => Set<WealthAllocation>();
    public DbSet<FinanceSimulation> FinanceSimulations => Set<FinanceSimulation>();
    public DbSet<ExtraIncome> ExtraIncomes => Set<ExtraIncome>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("pgcrypto");

        const string genUuid = "gen_random_uuid()";
        const string now = "now()";
        const string emptyJson = "'{}'::jsonb";

        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.DisplayName).HasMaxLength(120);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<WebauthnCredential>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.SignCount).HasDefaultValue(0L);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.CredentialId).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Ticker>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Symbol).HasMaxLength(20);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Exchange).HasMaxLength(20);
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.SecCik).HasMaxLength(10);
            e.Property(x => x.Country).HasColumnType("char(2)");
            e.Property(x => x.AssetType).HasMaxLength(20).HasDefaultValue("EQUITY");
            e.Property(x => x.DiscoverySource).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.Symbol, x.Exchange }).IsUnique();
        });

        b.Entity<PriceHistory>(e =>
        {
            e.HasKey(x => new { x.TickerId, x.Date });
            e.Property(x => x.Open).HasPrecision(18, 6);
            e.Property(x => x.High).HasPrecision(18, 6);
            e.Property(x => x.Low).HasPrecision(18, 6);
            e.Property(x => x.Close).HasPrecision(18, 6);
            e.Property(x => x.Source).HasMaxLength(30);
            e.HasIndex(x => new { x.TickerId, x.Date }).IsDescending(false, true)
                .HasDatabaseName("ix_price_history_ticker_date_desc");
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Fundamental>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Period).HasMaxLength(10);
            foreach (string p in new[]
            {
                nameof(Fundamental.Revenue), nameof(Fundamental.NetIncome), nameof(Fundamental.GrossProfit),
                nameof(Fundamental.Ebit), nameof(Fundamental.Ebitda), nameof(Fundamental.TotalAssets),
                nameof(Fundamental.TotalLiabilities), nameof(Fundamental.TotalEquity), nameof(Fundamental.RetainedEarnings),
                nameof(Fundamental.CurrentAssets), nameof(Fundamental.CurrentLiabilities), nameof(Fundamental.LongTermDebt),
                nameof(Fundamental.TotalDebt), nameof(Fundamental.Cash), nameof(Fundamental.OperatingCashFlow),
                nameof(Fundamental.Capex), nameof(Fundamental.FreeCashFlow), nameof(Fundamental.AccountsReceivable),
                nameof(Fundamental.PpeNet), nameof(Fundamental.Depreciation), nameof(Fundamental.SgaExpense),
            })
            {
                e.Property(p).HasPrecision(20, 2);
            }
            e.Property(x => x.Eps).HasPrecision(18, 6);
            e.Property(x => x.DividendPerShare).HasPrecision(18, 6);
            e.Property(x => x.BookValuePerShare).HasPrecision(18, 6);
            e.Property(x => x.DividendYield).HasPrecision(10, 6);
            e.Property(x => x.DividendGrowth).HasPrecision(10, 6);
            e.Property(x => x.MarketCap).HasPrecision(24, 2);
            e.Property(x => x.PeRatio).HasPrecision(12, 4);
            e.Property(x => x.Roe).HasPrecision(12, 6);
            e.Property(x => x.Raw).HasColumnType("jsonb").HasDefaultValueSql(emptyJson);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.TickerId, x.AsOfDate, x.Period }).IsUnique();
            e.HasIndex(x => x.TickerId);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<NewsItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Title).HasMaxLength(500);
            e.Property(x => x.Url).HasMaxLength(1000);
            e.Property(x => x.Sentiment).HasPrecision(5, 4);
            e.Property(x => x.Source).HasMaxLength(100);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.Url).IsUnique();
            e.HasIndex(x => new { x.TickerId, x.PublishedAt }).IsDescending(false, true)
                .HasDatabaseName("ix_news_items_ticker_published");
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SecFiling>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.FormType).HasMaxLength(20);
            e.Property(x => x.AccessionNo).HasMaxLength(30);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.AccessionNo).IsUnique();
            e.HasIndex(x => new { x.TickerId, x.FilingDate }).IsDescending(false, true)
                .HasDatabaseName("ix_sec_filings_ticker_filing_date");
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<InsiderTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Insider).HasMaxLength(200);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Shares).HasPrecision(20, 4);
            e.Property(x => x.Price).HasPrecision(18, 6);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.TickerId, x.TxDate }).IsDescending(false, true)
                .HasDatabaseName("ix_insider_transactions_ticker_tx_date");
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<AlgorithmScore>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.AlgorithmName).HasMaxLength(100);
            e.Property(x => x.Philosophy).HasMaxLength(30);
            e.Property(x => x.Score).HasPrecision(10, 4);
            e.Property(x => x.Details).HasColumnType("jsonb").HasDefaultValueSql(emptyJson);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_algorithm_scores_philosophy",
                "philosophy IN ('ALL', 'BALANCED', 'MOMENTUM', 'MEAN_REVERSION', 'FUNDAMENTAL')"));
            e.HasIndex(x => new { x.TickerId, x.AlgorithmName, x.AsOfDate, x.Philosophy }).IsUnique()
                .HasDatabaseName("ix_algorithm_scores_ticker_algo_phil_date");
            e.HasIndex(x => x.TickerId);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Transaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Side).HasMaxLength(4);
            e.Property(x => x.Shares).HasPrecision(20, 8);
            e.Property(x => x.Price).HasPrecision(18, 6);
            e.Property(x => x.Fee).HasPrecision(18, 6).HasDefaultValue(0m);
            e.Property(x => x.PriceAuto).HasDefaultValue(false);
            e.Property(x => x.FxRateAtExecution).HasPrecision(18, 6);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.Property(x => x.Broker).HasMaxLength(64).IsRequired().HasDefaultValue("TradeVille");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_transactions_side", "side IN ('BUY', 'SELL')");
                t.HasCheckConstraint("ck_transactions_shares", "shares > 0");
                t.HasCheckConstraint("ck_transactions_price", "price >= 0");
            });
            e.HasIndex(x => new { x.UserId, x.ExecutedAt }).IsDescending(false, true)
                .HasDatabaseName("ix_transactions_user_executed");
            e.HasIndex(x => x.TickerId);
            e.HasIndex(x => new { x.UserId, x.Broker }).HasDatabaseName("ix_transactions_user_broker");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Holding>(e =>
        {
            e.HasKey(x => new { x.UserId, x.TickerId });
            e.Property(x => x.Shares).HasPrecision(20, 8).HasDefaultValue(0m);
            e.Property(x => x.AvgCost).HasPrecision(18, 6).HasDefaultValue(0m);
            e.Property(x => x.RealizedPnl).HasPrecision(20, 6).HasDefaultValue(0m);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.Property(x => x.Broker).HasMaxLength(64).IsRequired().HasDefaultValue("TradeVille");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<WatchlistEntry>(e =>
        {
            e.HasKey(x => new { x.UserId, x.TickerId });
            e.Property(x => x.AddedAt).HasDefaultValueSql(now);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<PortfolioTarget>(e =>
        {
            e.HasKey(x => new { x.UserId, x.TickerId });
            e.Property(x => x.TargetShares).HasPrecision(20, 8);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_portfolio_targets_target_shares", "target_shares >= 0"));
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TickerNote>(e =>
        {
            e.HasKey(x => new { x.UserId, x.TickerId });
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Type).HasMaxLength(40);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.IsRead).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.UserId, x.CreatedAt }).IsDescending(false, true)
                .HasDatabaseName("ix_notifications_user_created");
            e.HasIndex(x => x.UserId).HasFilter("is_read = false")
                .HasDatabaseName("ix_notifications_user_unread");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<UserSetting>(e =>
        {
            e.HasKey(x => new { x.UserId, x.Key });
            e.Property(x => x.Key).HasMaxLength(60);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<SyncJob>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Source).HasMaxLength(30);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.StartedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_sync_jobs_status",
                "status IN ('PENDING', 'RUNNING', 'SUCCEEDED', 'FAILED')"));
            e.HasIndex(x => new { x.Source, x.StartedAt }).IsDescending(false, true)
                .HasDatabaseName("ix_sync_jobs_source_started");
        });

        b.Entity<ApiCredential>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Provider).HasMaxLength(30);
            e.Property(x => x.RotatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.Provider).IsUnique();
        });

        b.Entity<ExchangeRate>(e =>
        {
            e.HasKey(x => new { x.Currency, x.RateDate });
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.RateToRon).HasPrecision(18, 6);
            e.Property(x => x.Source).HasMaxLength(30);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.Currency, x.RateDate }).IsDescending(false, true)
                .HasDatabaseName("ix_exchange_rates_currency_date_desc");
        });

        b.Entity<TickerDividend>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.DividendYield).HasPrecision(10, 6);
            e.Property(x => x.ForwardDividend).HasPrecision(18, 6);
            e.Property(x => x.PayFrequency).HasMaxLength(20);
            e.Property(x => x.HistGrowthCagr).HasPrecision(10, 6);
            e.Property(x => x.Raw).HasColumnType("jsonb").HasDefaultValueSql(emptyJson);
            e.Property(x => x.SourceUrl).HasMaxLength(1000);
            e.Property(x => x.FetchedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_ticker_dividends_pay_frequency",
                "pay_frequency IS NULL OR pay_frequency IN ('MONTHLY', 'QUARTERLY', 'SEMI_ANNUAL', 'ANNUAL', 'IRREGULAR')"));
            e.HasIndex(x => x.TickerId).IsUnique();
            e.HasOne<Ticker>().WithMany().HasForeignKey(x => x.TickerId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BrokerFeeSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Broker).HasMaxLength(20);
            e.Property(x => x.Market).HasMaxLength(20);
            e.Property(x => x.Percent).HasPrecision(10, 6);
            e.Property(x => x.MinFee).HasPrecision(18, 6);
            e.Property(x => x.FixedFee).HasPrecision(18, 6);
            e.Property(x => x.FxConversionPercent).HasPrecision(10, 6);
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.Raw).HasColumnType("jsonb").HasDefaultValueSql(emptyJson);
            e.Property(x => x.SourceUrl).HasMaxLength(1000);
            e.Property(x => x.FetchedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_broker_fee_schedules_broker",
                "broker IN ('TRADEVILLE', 'XTB', 'IBKR')"));
            e.HasIndex(x => new { x.Broker, x.Market }).IsUnique();
        });

        b.Entity<DividendTaxRate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.CountryCode).HasColumnType("char(2)");
            e.Property(x => x.RatePercent).HasPrecision(10, 6);
            e.Property(x => x.SourceUrl).HasMaxLength(1000);
            e.Property(x => x.FetchedAt).HasDefaultValueSql(now);
            e.HasIndex(x => x.CountryCode).IsUnique();
        });

        const string emptyJsonArray = "'[]'::jsonb";

        b.Entity<FinanceSetting>(e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.MonthlyIncomeAmount).HasPrecision(20, 2).HasDefaultValue(0m);
            e.Property(x => x.MonthlyIncomeCurrency).HasColumnType("char(3)");
            e.Property(x => x.Banks).HasColumnType("jsonb").HasDefaultValueSql(emptyJsonArray);
            e.Property(x => x.Brokers).HasColumnType("jsonb").HasDefaultValueSql(emptyJsonArray);
            e.Property(x => x.CategoryTypes).HasColumnType("jsonb").HasDefaultValueSql(emptyJsonArray);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // The category "type" bucket used to be a fixed 7-value CHECK constraint; it's now user-extensible
        // (custom types live in finance_settings.category_types), so the DB no longer constrains the value —
        // FinanceService.NormalizeCategoryType is the validation boundary (builtin types ∪ the user's custom ones).
        b.Entity<FinanceCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Name).HasMaxLength(120);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.IsDefault).HasDefaultValue(false);
            e.HasIndex(x => new { x.UserId, x.Name, x.Type }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Expense>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Month).HasColumnType("char(7)");
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(120);
            e.Property(x => x.Amount).HasPrecision(20, 2);
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Status).HasMaxLength(10);
            e.Property(x => x.InterestRatePct).HasPrecision(10, 4);
            e.Property(x => x.IsRecurring).HasDefaultValue(false);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.Property(x => x.LineItems).HasColumnType("jsonb");
            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_expenses_status", "status IN ('PAID', 'DUE', 'PENDING')");
                t.HasCheckConstraint("ck_expenses_amount", "amount >= 0");
            });
            e.HasIndex(x => new { x.UserId, x.Month })
                .HasDatabaseName("ix_expenses_user_month");
            e.HasIndex(x => new { x.UserId, x.IsRecurring })
                .HasDatabaseName("ix_expenses_user_recurring");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<WealthAllocation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Institution).HasMaxLength(100);
            e.Property(x => x.InstitutionType).HasMaxLength(10);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Amount).HasPrecision(20, 2);
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.InterestRatePct).HasPrecision(10, 4);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.ToTable(t =>
            {
                t.HasCheckConstraint("ck_wealth_allocations_institution_type",
                    "institution_type IN ('BANK', 'BROKER')");
                t.HasCheckConstraint("ck_wealth_allocations_type",
                    "type IN ('BANK_DEPOSIT', 'BROKER_CASH', 'DCA_TARGET')");
                t.HasCheckConstraint("ck_wealth_allocations_amount", "amount >= 0");
            });
            e.HasIndex(x => x.UserId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<FinanceSimulation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.BaseCurrency).HasColumnType("char(3)");
            e.Property(x => x.Config).HasColumnType("jsonb").HasDefaultValueSql(emptyJson);
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.Property(x => x.UpdatedAt).HasDefaultValueSql(now);
            e.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ExtraIncome>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql(genUuid);
            e.Property(x => x.Month).HasMaxLength(7);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Amount).HasPrecision(20, 2);
            e.Property(x => x.Currency).HasColumnType("char(3)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql(now);
            e.ToTable(t => t.HasCheckConstraint("ck_extra_incomes_amount", "amount > 0"));
            e.HasIndex(x => new { x.UserId, x.Month }).HasDatabaseName("ix_extra_incomes_user_month");
            e.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
