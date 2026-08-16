using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    month = table.Column<string>(type: "char(7)", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    category = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    interest_rate_pct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    is_recurring = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expenses", x => x.id);
                    table.CheckConstraint("ck_expenses_amount", "amount >= 0");
                    table.CheckConstraint("ck_expenses_status", "status IN ('PAID', 'DUE', 'PENDING')");
                    table.CheckConstraint("ck_expenses_type", "type IN ('LOAN', 'SUBSCRIPTION', 'UTILITY', 'VARIABLE', 'FOOD', 'CIGARETTES', 'OTHER')");
                    table.ForeignKey(
                        name: "fk_expenses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "finance_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finance_categories", x => x.id);
                    table.CheckConstraint("ck_finance_categories_type", "type IN ('LOAN', 'SUBSCRIPTION', 'UTILITY', 'VARIABLE', 'FOOD', 'CIGARETTES', 'OTHER')");
                    table.ForeignKey(
                        name: "fk_finance_categories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "finance_settings",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monthly_income_amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false, defaultValue: 0m),
                    monthly_income_currency = table.Column<string>(type: "char(3)", nullable: false),
                    banks = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    brokers = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finance_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_finance_settings_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wealth_allocations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    institution = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    institution_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "char(3)", nullable: false),
                    interest_rate_pct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wealth_allocations", x => x.id);
                    table.CheckConstraint("ck_wealth_allocations_amount", "amount >= 0");
                    table.CheckConstraint("ck_wealth_allocations_institution_type", "institution_type IN ('BANK', 'BROKER')");
                    table.CheckConstraint("ck_wealth_allocations_type", "type IN ('BANK_DEPOSIT', 'BROKER_CASH', 'DCA_TARGET')");
                    table.ForeignKey(
                        name: "fk_wealth_allocations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_expenses_user_month",
                table: "expenses",
                columns: new[] { "user_id", "month" });

            migrationBuilder.CreateIndex(
                name: "ix_expenses_user_recurring",
                table: "expenses",
                columns: new[] { "user_id", "is_recurring" });

            migrationBuilder.CreateIndex(
                name: "ix_finance_categories_user_id",
                table: "finance_categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_finance_categories_user_id_name_type",
                table: "finance_categories",
                columns: new[] { "user_id", "name", "type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_wealth_allocations_user_id",
                table: "wealth_allocations",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "finance_categories");

            migrationBuilder.DropTable(
                name: "finance_settings");

            migrationBuilder.DropTable(
                name: "wealth_allocations");
        }
    }
}
