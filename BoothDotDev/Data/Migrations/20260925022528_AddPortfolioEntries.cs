using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "portfolio_entry",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    featured_position = table.Column<int>(type: "integer", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_entry", x => x.id);
                    table.CheckConstraint("ck_portfolio_entry_one_target", "(project_id IS NULL) <> (creation_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_portfolio_entry_creation_creation_id",
                        column: x => x.creation_id,
                        principalSchema: "public",
                        principalTable: "creation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_entry_project_project_id",
                        column: x => x.project_id,
                        principalSchema: "public",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_entry_creation_id",
                schema: "public",
                table: "portfolio_entry",
                column: "creation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_entry_project_id",
                schema: "public",
                table: "portfolio_entry",
                column: "project_id",
                unique: true);

            // list everything the portfolio page shows today, in the order it shows it (creations newest first, then projects
            // by status, rank and name), and feature what the home page shows today (the two newest creations, then projects
            // up to six in all)
            migrationBuilder.Sql("""
                WITH shown_creations AS (
                    SELECT id, row_number() OVER (ORDER BY published_at DESC) - 1 AS n
                    FROM public.creation
                    WHERE visibility = 'published' AND trashed_at IS NULL
                ),
                ordered_projects AS (
                    SELECT id, row_number() OVER (
                        ORDER BY CASE status WHEN 'ongoing' THEN 0 WHEN 'past' THEN 1 WHEN 'hiatus' THEN 2 ELSE 3 END, "rank", name) - 1 AS n
                    FROM public.project
                ),
                totals AS (
                    SELECT count(*) AS creations FROM shown_creations
                )
                INSERT INTO public.portfolio_entry (id, "position", featured_position, project_id, creation_id)
                SELECT gen_random_uuid(), c.n::int, CASE WHEN c.n < 2 THEN c.n::int END, NULL, c.id
                FROM shown_creations c
                UNION ALL
                SELECT gen_random_uuid(),
                       ((SELECT creations FROM totals) + p.n)::int,
                       CASE WHEN p.n < 6 - LEAST(2, (SELECT creations FROM totals))
                            THEN (LEAST(2, (SELECT creations FROM totals)) + p.n)::int END,
                       p.id, NULL
                FROM ordered_projects p;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "portfolio_entry",
                schema: "public");
        }
    }
}
