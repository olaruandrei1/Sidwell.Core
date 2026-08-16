using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerToPortfolio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "broker",
                table: "transactions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "TradeVille");

            migrationBuilder.AddColumn<string>(
                name: "broker",
                table: "holdings",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "TradeVille");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_user_broker",
                table: "transactions",
                columns: new[] { "user_id", "broker" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_transactions_user_broker",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "broker",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "broker",
                table: "holdings");
        }
    }
}
