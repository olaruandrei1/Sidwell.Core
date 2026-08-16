using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTickerNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS ticker_journal_notes (
                    id UUID NOT NULL DEFAULT gen_random_uuid(),
                    ticker_id UUID NOT NULL,
                    user_id UUID NOT NULL,
                    title CHARACTER VARYING(200) NOT NULL DEFAULT '',
                    sections JSONB NOT NULL DEFAULT '[]',
                    attachments JSONB NOT NULL DEFAULT '[]',
                    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
                    updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
                    CONSTRAINT pk_ticker_journal_notes PRIMARY KEY (id),
                    CONSTRAINT fk_ticker_journal_notes_ticker FOREIGN KEY (ticker_id) REFERENCES tickers(id) ON DELETE CASCADE,
                    CONSTRAINT fk_ticker_journal_notes_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS ix_ticker_journal_notes_ticker_user ON ticker_journal_notes (ticker_id, user_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS ticker_journal_notes;");
        }
    }
}
