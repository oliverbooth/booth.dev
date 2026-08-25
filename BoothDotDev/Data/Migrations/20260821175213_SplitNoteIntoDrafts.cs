using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitNoteIntoDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1: expand. Add the new table and pointer column; nothing existing is touched yet. ---

            migrationBuilder.CreateTable(
                name: "note_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    font_style = table.Column<FontStyle>(type: "public.font_style", nullable: false, defaultValue: FontStyle.Serif),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_note_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_note_draft_note_note_id",
                        column: x => x.note_id,
                        principalSchema: "public",
                        principalTable: "note",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_note_draft_note_id",
                schema: "public",
                table: "note_draft",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "ix_note_draft_created_at",
                schema: "public",
                table: "note_draft",
                column: "created_at");

            migrationBuilder.AddColumn<Guid>(
                name: "current_draft_id",
                schema: "public",
                table: "note",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_note_current_draft_id",
                schema: "public",
                table: "note",
                column: "current_draft_id");

            migrationBuilder.AddForeignKey(
                name: "fk_note_note_draft_current_draft_id",
                schema: "public",
                table: "note",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "note_draft",
                principalColumn: "id");

            // --- Phase 2: migrate. Give every existing note a generation-zero draft carrying its current
            // content, then point it at that draft. Nothing here is hand-typed — Postgres copies every row
            // in one pass. ---

            migrationBuilder.Sql(
                """
                INSERT INTO public.note_draft (id, note_id, title, content, font_style, visibility, created_at)
                SELECT gen_random_uuid(), id, title, content, font_style, visibility, COALESCE(updated, published)
                FROM public.note;

                UPDATE public.note n
                SET current_draft_id = nd.id
                FROM public.note_draft nd
                WHERE nd.note_id = n.id;
                """);

            // --- Phase 3: contract. Every note now has a current draft holding its old content, so the
            // columns that content used to live in on `note` are safe to drop. ---

            migrationBuilder.DropColumn(
                name: "content",
                schema: "public",
                table: "note");

            migrationBuilder.DropColumn(
                name: "font_style",
                schema: "public",
                table: "note");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "public",
                table: "note");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "public",
                table: "note");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Reverse of phase 3: bring the old columns back. ---

            migrationBuilder.AddColumn<string>(
                name: "content",
                schema: "public",
                table: "note",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<FontStyle>(
                name: "font_style",
                schema: "public",
                table: "note",
                type: "public.font_style",
                nullable: false,
                defaultValue: FontStyle.Serif);

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "public",
                table: "note",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "note",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Published);

            // Backfill the resurrected columns from each note's current draft, while both `current_draft_id`
            // and `note_draft` still exist. This only restores the *current* draft's content — the rest of
            // the draft history has nowhere to go in the old schema and is discarded here.

            migrationBuilder.Sql(
                """
                UPDATE public.note n
                SET title = nd.title,
                    content = nd.content,
                    font_style = nd.font_style,
                    visibility = nd.visibility
                FROM public.note_draft nd
                WHERE nd.id = n.current_draft_id;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_note_note_draft_current_draft_id",
                schema: "public",
                table: "note");

            migrationBuilder.DropTable(
                name: "note_draft",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_note_current_draft_id",
                schema: "public",
                table: "note");

            migrationBuilder.DropColumn(
                name: "current_draft_id",
                schema: "public",
                table: "note");
        }
    }
}
