using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerFxConversionPercent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "fx_conversion_percent",
                table: "broker_fee_schedules",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fx_conversion_percent",
                table: "broker_fee_schedules");
        }
    }
}
