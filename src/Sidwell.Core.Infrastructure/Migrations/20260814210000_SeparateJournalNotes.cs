using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateJournalNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reconciles the ticker_notes name collision between two features:
            //   - legacy single-body research note (ticker_notes: user_id, ticker_id, body)
            //   - new research journal (ticker_journal_notes: id, title, sections, attachments)
            // AddTickerNotes previously recreated ticker_notes with the journal schema,
            // which broke the legacy note read. This idempotent block restores both.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    -- If ticker_notes currently holds the journal schema, rename it to its own table.
                    IF EXISTS (SELECT 1 FROM information_schema.columns
                               WHERE table_schema = 'public' AND table_name = 'ticker_notes' AND column_name = 'title') THEN
                        ALTER TABLE ticker_notes RENAME TO ticker_journal_notes;
                        ALTER TABLE ticker_journal_notes RENAME CONSTRAINT pk_ticker_notes TO pk_ticker_journal_notes;
                        ALTER TABLE ticker_journal_notes RENAME CONSTRAINT fk_ticker_notes_ticker TO fk_ticker_journal_notes_ticker;
                        ALTER TABLE ticker_journal_notes RENAME CONSTRAINT fk_ticker_notes_user TO fk_ticker_journal_notes_user;
                        ALTER INDEX ix_ticker_notes_ticker_user RENAME TO ix_ticker_journal_notes_ticker_user;
                    END IF;

                    -- Ensure the journal table exists (safety net for fresh databases).
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables
                                   WHERE table_schema = 'public' AND table_name = 'ticker_journal_notes') THEN
                        CREATE TABLE ticker_journal_notes (
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
                        CREATE INDEX ix_ticker_journal_notes_ticker_user ON ticker_journal_notes (ticker_id, user_id);
                    END IF;

                    -- Restore the legacy single-body research note table if it is missing.
                    IF NOT EXISTS (SELECT 1 FROM information_schema.tables
                                   WHERE table_schema = 'public' AND table_name = 'ticker_notes') THEN
                        CREATE TABLE ticker_notes (
                            user_id UUID NOT NULL,
                            ticker_id UUID NOT NULL,
                            body TEXT NOT NULL DEFAULT '',
                            updated_at TIMESTAMPTZ NOT NULL DEFAULT now(),
                            CONSTRAINT pk_ticker_notes PRIMARY KEY (user_id, ticker_id),
                            CONSTRAINT fk_ticker_notes_tickers_ticker_id FOREIGN KEY (ticker_id) REFERENCES tickers(id) ON DELETE CASCADE,
                            CONSTRAINT fk_ticker_notes_users_user_id FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                        );
                        CREATE INDEX ix_ticker_notes_ticker_id ON ticker_notes (ticker_id);
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS ticker_journal_notes;");
        }
    }
}
