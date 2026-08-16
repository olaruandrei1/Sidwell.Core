using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelaxWealthAmountCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Allow negative amounts so withdrawals (expense paid from deposit/portfolio)
            // can be recorded as negative wealth_allocation rows for the same month.
            migrationBuilder.Sql("""
                ALTER TABLE wealth_allocations DROP CONSTRAINT IF EXISTS ck_wealth_allocations_amount;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE wealth_allocations ADD CONSTRAINT ck_wealth_allocations_amount CHECK (amount >= 0);
                """);
        }
    }
}
