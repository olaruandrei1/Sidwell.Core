using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "algorithms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_philosophy_dependent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_algorithms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_credentials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    provider = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    encrypted_key = table.Column<string>(type: "text", nullable: false),
                    rotated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_credentials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    currency = table.Column<string>(type: "char(3)", nullable: false),
                    rate_date = table.Column<DateOnly>(type: "date", nullable: false),
                    rate_to_ron = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exchange_rates", x => new { x.currency, x.rate_date });
                });

            migrationBuilder.CreateTable(
                name: "sync_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sync_jobs", x => x.id);
                    table.CheckConstraint("ck_sync_jobs_status", "status IN ('PENDING', 'RUNNING', 'SUCCEEDED', 'FAILED')");
                });

            migrationBuilder.CreateTable(
                name: "tickers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    exchange = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false),
                    sec_cik = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tickers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "algorithm_scores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    algorithm_id = table.Column<Guid>(type: "uuid", nullable: false),
                    philosophy = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    as_of_date = table.Column<DateOnly>(type: "date", nullable: false),
                    score = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    details = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_algorithm_scores", x => x.id);
                    table.CheckConstraint("ck_algorithm_scores_philosophy", "philosophy IN ('ALL', 'BALANCED', 'MOMENTUM', 'MEAN_REVERSION', 'FUNDAMENTAL')");
                    table.ForeignKey(
                        name: "fk_algorithm_scores_algorithms_algorithm_id",
                        column: x => x.algorithm_id,
                        principalTable: "algorithms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_algorithm_scores_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fundamentals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    as_of_date = table.Column<DateOnly>(type: "date", nullable: false),
                    period = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    revenue = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    net_income = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    gross_profit = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    ebit = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    ebitda = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    total_assets = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    total_liabilities = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    total_equity = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    retained_earnings = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    current_assets = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    current_liabilities = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    long_term_debt = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    total_debt = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    cash = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    operating_cash_flow = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    capex = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    free_cash_flow = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    eps = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    shares_outstanding = table.Column<long>(type: "bigint", nullable: true),
                    dividend_per_share = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    dividend_yield = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    dividend_growth = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    book_value_per_share = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    market_cap = table.Column<decimal>(type: "numeric(24,2)", precision: 24, scale: 2, nullable: true),
                    pe_ratio = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    roe = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: true),
                    accounts_receivable = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    ppe_net = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    depreciation = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    sga_expense = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    raw = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fundamentals", x => x.id);
                    table.ForeignKey(
                        name: "fk_fundamentals_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "insider_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    insider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    shares = table.Column<decimal>(type: "numeric(20,4)", precision: 20, scale: 4, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    tx_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_insider_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_insider_transactions_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "news_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sentiment = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_news_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_news_items_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_history",
                columns: table => new
                {
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_history", x => new { x.ticker_id, x.date });
                    table.ForeignKey(
                        name: "fk_price_history_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sec_filings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    filing_date = table.Column<DateOnly>(type: "date", nullable: false),
                    accession_no = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sec_filings", x => x.id);
                    table.ForeignKey(
                        name: "fk_sec_filings_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "holdings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shares = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false, defaultValue: 0m),
                    avg_cost = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    realized_pnl = table.Column<decimal>(type: "numeric(20,6)", precision: 20, scale: 6, nullable: false, defaultValue: 0m),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_holdings", x => new { x.user_id, x.ticker_id });
                    table.ForeignKey(
                        name: "fk_holdings_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_holdings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_targets",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_shares = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_targets", x => new { x.user_id, x.ticker_id });
                    table.CheckConstraint("ck_portfolio_targets_target_shares", "target_shares >= 0");
                    table.ForeignKey(
                        name: "fk_portfolio_targets_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_targets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screener_presets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    criteria = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_screener_presets", x => x.id);
                    table.ForeignKey(
                        name: "fk_screener_presets_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ticker_notes",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticker_notes", x => new { x.user_id, x.ticker_id });
                    table.ForeignKey(
                        name: "fk_ticker_notes_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_ticker_notes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    side = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    shares = table.Column<decimal>(type: "numeric(20,8)", precision: 20, scale: 8, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    fee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    fx_rate_at_execution = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transactions", x => x.id);
                    table.CheckConstraint("ck_transactions_price", "price >= 0");
                    table.CheckConstraint("ck_transactions_shares", "shares > 0");
                    table.CheckConstraint("ck_transactions_side", "side IN ('BUY', 'SELL')");
                    table.ForeignKey(
                        name: "fk_transactions_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_settings", x => new { x.user_id, x.key });
                    table.ForeignKey(
                        name: "fk_user_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "watchlist",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchlist", x => new { x.user_id, x.ticker_id });
                    table.ForeignKey(
                        name: "fk_watchlist_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_watchlist_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "webauthn_credentials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    credential_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    sign_count = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_webauthn_credentials", x => x.id);
                    table.ForeignKey(
                        name: "fk_webauthn_credentials_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_algorithm_scores_algorithm_id",
                table: "algorithm_scores",
                column: "algorithm_id");

            migrationBuilder.CreateIndex(
                name: "ix_algorithm_scores_ticker_id",
                table: "algorithm_scores",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_algorithm_scores_ticker_id_algorithm_id_philosophy_as_of_da",
                table: "algorithm_scores",
                columns: new[] { "ticker_id", "algorithm_id", "philosophy", "as_of_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_algorithms_name_version",
                table: "algorithms",
                columns: new[] { "name", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_api_credentials_provider",
                table: "api_credentials",
                column: "provider",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exchange_rates_currency_date_desc",
                table: "exchange_rates",
                columns: new[] { "currency", "rate_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_fundamentals_ticker_id",
                table: "fundamentals",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_fundamentals_ticker_id_as_of_date_period",
                table: "fundamentals",
                columns: new[] { "ticker_id", "as_of_date", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_holdings_ticker_id",
                table: "holdings",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_insider_transactions_ticker_tx_date",
                table: "insider_transactions",
                columns: new[] { "ticker_id", "tx_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_news_items_ticker_published",
                table: "news_items",
                columns: new[] { "ticker_id", "published_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_news_items_url",
                table: "news_items",
                column: "url",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_created",
                table: "notifications",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_unread",
                table: "notifications",
                column: "user_id",
                filter: "is_read = false");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_targets_ticker_id",
                table: "portfolio_targets",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_price_history_ticker_date_desc",
                table: "price_history",
                columns: new[] { "ticker_id", "date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_screener_presets_user_id",
                table: "screener_presets",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_screener_presets_user_id_name",
                table: "screener_presets",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sec_filings_accession_no",
                table: "sec_filings",
                column: "accession_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sec_filings_ticker_filing_date",
                table: "sec_filings",
                columns: new[] { "ticker_id", "filing_date" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_sync_jobs_source_started",
                table: "sync_jobs",
                columns: new[] { "source", "started_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_ticker_notes_ticker_id",
                table: "ticker_notes",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickers_symbol_exchange",
                table: "tickers",
                columns: new[] { "symbol", "exchange" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transactions_ticker_id",
                table: "transactions",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_user_executed",
                table: "transactions",
                columns: new[] { "user_id", "executed_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_watchlist_ticker_id",
                table: "watchlist",
                column: "ticker_id");

            migrationBuilder.CreateIndex(
                name: "ix_webauthn_credentials_credential_id",
                table: "webauthn_credentials",
                column: "credential_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_webauthn_credentials_user_id",
                table: "webauthn_credentials",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "algorithm_scores");

            migrationBuilder.DropTable(
                name: "api_credentials");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "fundamentals");

            migrationBuilder.DropTable(
                name: "holdings");

            migrationBuilder.DropTable(
                name: "insider_transactions");

            migrationBuilder.DropTable(
                name: "news_items");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "portfolio_targets");

            migrationBuilder.DropTable(
                name: "price_history");

            migrationBuilder.DropTable(
                name: "screener_presets");

            migrationBuilder.DropTable(
                name: "sec_filings");

            migrationBuilder.DropTable(
                name: "sync_jobs");

            migrationBuilder.DropTable(
                name: "ticker_notes");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "watchlist");

            migrationBuilder.DropTable(
                name: "webauthn_credentials");

            migrationBuilder.DropTable(
                name: "algorithms");

            migrationBuilder.DropTable(
                name: "tickers");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
