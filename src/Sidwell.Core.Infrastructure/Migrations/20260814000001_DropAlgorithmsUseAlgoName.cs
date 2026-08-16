using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sidwell.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropAlgorithmsUseAlgoName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE algorithm_scores DROP CONSTRAINT fk_algorithm_scores_algorithms_algorithm_id;
                DROP INDEX ix_algorithm_scores_algorithm_id;
                DROP INDEX ix_algorithm_scores_ticker_id_algorithm_id_philosophy_as_of_da;
                ALTER TABLE algorithm_scores ADD COLUMN algorithm_name character varying(100) NOT NULL DEFAULT '';
                UPDATE algorithm_scores a
                    SET algorithm_name = al.name
                    FROM algorithms al
                    WHERE al.id = a.algorithm_id;
                ALTER TABLE algorithm_scores ALTER COLUMN algorithm_name DROP DEFAULT;
                ALTER TABLE algorithm_scores DROP COLUMN algorithm_id;
                CREATE UNIQUE INDEX ix_algorithm_scores_ticker_algo_phil_date
                    ON algorithm_scores (ticker_id, algorithm_name, as_of_date, philosophy);
                DROP TABLE algorithms;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.InvalidOperationException("This migration cannot be reversed — algorithms table has been removed.");
        }
    }
}
