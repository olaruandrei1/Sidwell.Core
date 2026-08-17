using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomCategoryTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_finance_categories_type",
                table: "finance_categories");

            migrationBuilder.DropCheckConstraint(
                name: "ck_expenses_type",
                table: "expenses");

            migrationBuilder.AddColumn<string>(
                name: "category_types",
                table: "finance_settings",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category_types",
                table: "finance_settings");

            migrationBuilder.AddCheckConstraint(
                name: "ck_finance_categories_type",
                table: "finance_categories",
                sql: "type IN ('LOAN', 'SUBSCRIPTION', 'UTILITY', 'VARIABLE', 'FOOD', 'CIGARETTES', 'OTHER')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_expenses_type",
                table: "expenses",
                sql: "type IN ('LOAN', 'SUBSCRIPTION', 'UTILITY', 'VARIABLE', 'FOOD', 'CIGARETTES', 'OTHER')");
        }
    }
}
