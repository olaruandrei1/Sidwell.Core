using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWealthAllocationMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE wealth_allocations ADD COLUMN IF NOT EXISTS month char(7);
                UPDATE wealth_allocations SET month = to_char(now(), 'YYYY-MM') WHERE month IS NULL;
                ALTER TABLE wealth_allocations ALTER COLUMN month SET NOT NULL;
                CREATE INDEX IF NOT EXISTS ix_wealth_allocations_user_month
                    ON wealth_allocations(user_id, month);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ix_wealth_allocations_user_month;
                ALTER TABLE wealth_allocations DROP COLUMN IF EXISTS month;
                """);
        }
    }
}
