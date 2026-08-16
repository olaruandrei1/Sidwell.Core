using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTickerDiscoveryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "asset_type",
                table: "tickers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "EQUITY");

            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "tickers",
                type: "char(2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "discovered_at",
                table: "tickers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "discovery_source",
                table: "tickers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "asset_type",
                table: "tickers");

            migrationBuilder.DropColumn(
                name: "country",
                table: "tickers");

            migrationBuilder.DropColumn(
                name: "discovered_at",
                table: "tickers");

            migrationBuilder.DropColumn(
                name: "discovery_source",
                table: "tickers");
        }
    }
}
