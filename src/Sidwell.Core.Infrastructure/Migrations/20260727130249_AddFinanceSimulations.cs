using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceSimulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "finance_simulations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    horizon_year = table.Column<int>(type: "integer", nullable: false),
                    base_currency = table.Column<string>(type: "char(3)", nullable: false),
                    config = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_finance_simulations", x => x.id);
                    table.ForeignKey(
                        name: "fk_finance_simulations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_finance_simulations_user_id",
                table: "finance_simulations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_finance_simulations_user_id_name",
                table: "finance_simulations",
                columns: new[] { "user_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "finance_simulations");
        }
    }
}
