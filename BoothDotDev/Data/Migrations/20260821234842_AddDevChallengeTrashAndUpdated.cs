using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDevChallengeTrashAndUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1: expand. New columns/table/pointer; nothing existing is touched yet. ---

            migrationBuilder.AddColumn<Guid>(
                name: "current_draft_id",
                schema: "public",
                table: "dev_challenge",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "dev_challenge",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated",
                schema: "public",
                table: "dev_challenge",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dev_challenge_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    dev_challenge_id = table.Column<Guid>(type: "uuid", nullable: false),
                    show_solution = table.Column<bool>(type: "boolean", nullable: false),
                    solution = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dev_challenge_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_dev_challenge_draft_dev_challenge_dev_challenge_id",
                        column: x => x.dev_challenge_id,
                        principalSchema: "public",
                        principalTable: "dev_challenge",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dev_challenge_current_draft_id",
                schema: "public",
                table: "dev_challenge",
                column: "current_draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_dev_challenge_trashed_at",
                schema: "public",
                table: "dev_challenge",
                column: "trashed_at");

            migrationBuilder.CreateIndex(
                name: "ix_dev_challenge_draft_created_at",
                schema: "public",
                table: "dev_challenge_draft",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_dev_challenge_draft_dev_challenge_id",
                schema: "public",
                table: "dev_challenge_draft",
                column: "dev_challenge_id");

            migrationBuilder.AddForeignKey(
                name: "fk_dev_challenge_dev_challenge_drafts_current_draft_id",
                schema: "public",
                table: "dev_challenge",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "dev_challenge_draft",
                principalColumn: "id");

            // --- Phase 2: migrate. Give every existing challenge a generation-zero draft carrying its current
            // content, then point it at that draft. ---

            migrationBuilder.Sql(
                """
                INSERT INTO public.dev_challenge_draft (id, dev_challenge_id, title, description, solution, show_solution, visibility, created_at)
                SELECT gen_random_uuid(), id, title, description, solution, show_solution, visibility, COALESCE(updated, published_at)
                FROM public.dev_challenge;

                UPDATE public.dev_challenge dc
                SET current_draft_id = dcd.id
                FROM public.dev_challenge_draft dcd
                WHERE dcd.dev_challenge_id = dc.id;
                """);

            // --- Phase 3: contract. Every challenge now has a current draft holding its old content, so the
            // columns that content used to live in on `dev_challenge` are safe to drop. ---

            migrationBuilder.DropColumn(
                name: "description",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "show_solution",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "solution",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "public",
                table: "dev_challenge");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Reverse of phase 3: bring the old columns back. ---

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "public",
                table: "dev_challenge",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "show_solution",
                schema: "public",
                table: "dev_challenge",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "solution",
                schema: "public",
                table: "dev_challenge",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "public",
                table: "dev_challenge",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "dev_challenge",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Private);

            // Backfill the resurrected columns from each challenge's current draft, while both
            // `current_draft_id` and `dev_challenge_draft` still exist. This only restores the *current*
            // draft's content — the rest of the draft history has nowhere to go in the old schema and is
            // discarded here.

            migrationBuilder.Sql(
                """
                UPDATE public.dev_challenge dc
                SET title = dcd.title,
                    description = dcd.description,
                    solution = dcd.solution,
                    show_solution = dcd.show_solution,
                    visibility = dcd.visibility
                FROM public.dev_challenge_draft dcd
                WHERE dcd.id = dc.current_draft_id;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_dev_challenge_dev_challenge_drafts_current_draft_id",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropTable(
                name: "dev_challenge_draft",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_dev_challenge_current_draft_id",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropIndex(
                name: "ix_dev_challenge_trashed_at",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "current_draft_id",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "dev_challenge");

            migrationBuilder.DropColumn(
                name: "updated",
                schema: "public",
                table: "dev_challenge");
        }
    }
}
