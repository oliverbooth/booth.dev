using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitProjectDevlogIntoDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1: expand. Add the new table and pointer column; nothing existing is touched yet. ---

            migrationBuilder.CreateTable(
                name: "devlog_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_devlog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_devlog_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_devlog_draft_devlog_project_devlog_id",
                        column: x => x.project_devlog_id,
                        principalSchema: "public",
                        principalTable: "devlog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devlog_draft_project_devlog_id",
                schema: "public",
                table: "devlog_draft",
                column: "project_devlog_id");

            migrationBuilder.CreateIndex(
                name: "ix_devlog_draft_created_at",
                schema: "public",
                table: "devlog_draft",
                column: "created_at");

            migrationBuilder.AddColumn<Guid>(
                name: "current_draft_id",
                schema: "public",
                table: "devlog",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "devlog",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_devlog_current_draft_id",
                schema: "public",
                table: "devlog",
                column: "current_draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_devlog_trashed_at",
                schema: "public",
                table: "devlog",
                column: "trashed_at");

            migrationBuilder.CreateIndex(
                name: "ix_devlog_project_id",
                schema: "public",
                table: "devlog",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "fk_devlog_project_devlog_drafts_current_draft_id",
                schema: "public",
                table: "devlog",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "devlog_draft",
                principalColumn: "id");

            // --- Phase 2: migrate. Give every existing devlog entry a generation-zero draft carrying its
            // current content, then point it at that draft. Nothing here is hand-typed - Postgres copies
            // every row in one pass. ---

            migrationBuilder.Sql(
                """
                INSERT INTO public.devlog_draft
                    (id, project_devlog_id, created_at, title, body, visibility)
                SELECT gen_random_uuid(), id, COALESCE(updated_at, published_at), title, body, visibility
                FROM public.devlog;

                UPDATE public.devlog d
                SET current_draft_id = dd.id
                FROM public.devlog_draft dd
                WHERE dd.project_devlog_id = d.id;
                """);

            // --- Phase 3: contract. Every devlog entry now has a current draft holding its old content, so
            // the columns that content used to live in on `devlog` are safe to drop. ---

            migrationBuilder.DropColumn(
                name: "body",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "public",
                table: "devlog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Reverse of phase 3: bring the old columns back. ---

            migrationBuilder.AddColumn<string>(
                name: "body",
                schema: "public",
                table: "devlog",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "public",
                table: "devlog",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "devlog",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Private);

            // Backfill the resurrected columns from each devlog entry's current draft, while both
            // `current_draft_id` and `devlog_draft` still exist. This only restores the *current* draft's
            // content - the rest of the draft history has nowhere to go in the old schema and is discarded
            // here.

            migrationBuilder.Sql(
                """
                UPDATE public.devlog d
                SET title = dd.title,
                    body = dd.body,
                    visibility = dd.visibility
                FROM public.devlog_draft dd
                WHERE dd.id = d.current_draft_id;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_devlog_project_devlog_drafts_current_draft_id",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropTable(
                name: "devlog_draft",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_devlog_current_draft_id",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropIndex(
                name: "ix_devlog_trashed_at",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropIndex(
                name: "ix_devlog_project_id",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropColumn(
                name: "current_draft_id",
                schema: "public",
                table: "devlog");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "devlog");
        }
    }
}
