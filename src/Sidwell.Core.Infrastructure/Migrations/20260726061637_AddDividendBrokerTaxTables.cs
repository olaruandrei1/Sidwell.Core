using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDividendBrokerTaxTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "broker_fee_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    broker = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    market = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    percent = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    min_fee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    fixed_fee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    currency = table.Column<string>(type: "char(3)", nullable: true),
                    raw = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    source_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_broker_fee_schedules", x => x.id);
                    table.CheckConstraint("ck_broker_fee_schedules_broker", "broker IN ('TRADEVILLE', 'XTB', 'IBKR')");
                });

            migrationBuilder.CreateTable(
                name: "dividend_tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    country_code = table.Column<string>(type: "char(2)", nullable: false),
                    rate_percent = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    source_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dividend_tax_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticker_dividends",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ticker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dividend_yield = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    forward_dividend = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    ex_dividend_date = table.Column<DateOnly>(type: "date", nullable: true),
                    pay_frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    hist_growth_cagr = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    raw = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    source_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ticker_dividends", x => x.id);
                    table.CheckConstraint("ck_ticker_dividends_pay_frequency", "pay_frequency IS NULL OR pay_frequency IN ('MONTHLY', 'QUARTERLY', 'SEMI_ANNUAL', 'ANNUAL', 'IRREGULAR')");
                    table.ForeignKey(
                        name: "fk_ticker_dividends_tickers_ticker_id",
                        column: x => x.ticker_id,
                        principalTable: "tickers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_broker_fee_schedules_broker_market",
                table: "broker_fee_schedules",
                columns: new[] { "broker", "market" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dividend_tax_rates_country_code",
                table: "dividend_tax_rates",
                column: "country_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ticker_dividends_ticker_id",
                table: "ticker_dividends",
                column: "ticker_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broker_fee_schedules");

            migrationBuilder.DropTable(
                name: "dividend_tax_rates");

            migrationBuilder.DropTable(
                name: "ticker_dividends");
        }
    }
}
