using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseStatusOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS expense_status_overrides (
                    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    user_id uuid NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                    expense_id uuid NOT NULL REFERENCES expenses(id) ON DELETE CASCADE,
                    month char(7) NOT NULL,
                    status varchar(10) NOT NULL CHECK (status IN ('PAID','DUE','PENDING')),
                    updated_at timestamptz NOT NULL DEFAULT now(),
                    UNIQUE (user_id, expense_id, month)
                );
                CREATE INDEX IF NOT EXISTS ix_expense_status_overrides_user_month
                    ON expense_status_overrides(user_id, month);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS expense_status_overrides;");
        }
    }
}
