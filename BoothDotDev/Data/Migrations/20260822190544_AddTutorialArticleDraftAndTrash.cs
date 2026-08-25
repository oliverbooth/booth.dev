using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorialArticleDraftAndTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1: expand. Add the new table and pointer column; nothing existing is touched yet. ---

            migrationBuilder.CreateTable(
                name: "tutorial_article_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tutorial_article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    excerpt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    folder = table.Column<Guid>(type: "uuid", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    preview_image_url = table.Column<string>(type: "text", nullable: true),
                    show_toc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    toc_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tutorial_article_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_tutorial_article_draft_tutorial_article_tutorial_article_id",
                        column: x => x.tutorial_article_id,
                        principalSchema: "public",
                        principalTable: "tutorial_article",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_tutorial_article_draft_tutorial_folders_folder",
                        column: x => x.folder,
                        principalSchema: "public",
                        principalTable: "tutorial_folder",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_draft_tutorial_article_id",
                schema: "public",
                table: "tutorial_article_draft",
                column: "tutorial_article_id");

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_draft_created_at",
                schema: "public",
                table: "tutorial_article_draft",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_draft_folder",
                schema: "public",
                table: "tutorial_article_draft",
                column: "folder");

            migrationBuilder.AddColumn<Guid>(
                name: "current_draft_id",
                schema: "public",
                table: "tutorial_article",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "tutorial_article",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_current_draft_id",
                schema: "public",
                table: "tutorial_article",
                column: "current_draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_trashed_at",
                schema: "public",
                table: "tutorial_article",
                column: "trashed_at");

            migrationBuilder.AddForeignKey(
                name: "fk_tutorial_article_tutorial_article_drafts_current_draft_id",
                schema: "public",
                table: "tutorial_article",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "tutorial_article_draft",
                principalColumn: "id");

            // --- Phase 2: migrate. Give every existing article a generation-zero draft carrying its current
            // content, then point it at that draft. Nothing here is hand-typed — Postgres copies every row
            // in one pass. ---

            migrationBuilder.Sql(
                """
                INSERT INTO public.tutorial_article_draft
                    (id, tutorial_article_id, created_at, title, body, excerpt, folder, rank, preview_image_url, show_toc, toc_open, visibility)
                SELECT gen_random_uuid(), id, COALESCE(updated, published), title, body, excerpt, folder, rank, preview_image_url, show_toc, toc_open, visibility
                FROM public.tutorial_article;

                UPDATE public.tutorial_article a
                SET current_draft_id = ad.id
                FROM public.tutorial_article_draft ad
                WHERE ad.tutorial_article_id = a.id;
                """);

            // --- Phase 3: contract. Every article now has a current draft holding its old content, so the
            // columns that content used to live in on `tutorial_article` are safe to drop. ---

            migrationBuilder.DropForeignKey(
                name: "fk_tutorial_article_tutorial_folders_folder",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropIndex(
                name: "ix_tutorial_article_folder",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "body",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "excerpt",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "folder",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "preview_image_url",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "rank",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "show_toc",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "toc_open",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "public",
                table: "tutorial_article");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Reverse of phase 3: bring the old columns back. ---

            migrationBuilder.AddColumn<string>(
                name: "body",
                schema: "public",
                table: "tutorial_article",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "excerpt",
                schema: "public",
                table: "tutorial_article",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "folder",
                schema: "public",
                table: "tutorial_article",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "preview_image_url",
                schema: "public",
                table: "tutorial_article",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rank",
                schema: "public",
                table: "tutorial_article",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "show_toc",
                schema: "public",
                table: "tutorial_article",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "public",
                table: "tutorial_article",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "toc_open",
                schema: "public",
                table: "tutorial_article",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "tutorial_article",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Private);

            // Backfill the resurrected columns from each article's current draft, while both `current_draft_id`
            // and `tutorial_article_draft` still exist. This only restores the *current* draft's content — the
            // rest of the draft history has nowhere to go in the old schema and is discarded here.

            migrationBuilder.Sql(
                """
                UPDATE public.tutorial_article a
                SET title = ad.title,
                    body = ad.body,
                    excerpt = ad.excerpt,
                    folder = ad.folder,
                    rank = ad.rank,
                    preview_image_url = ad.preview_image_url,
                    show_toc = ad.show_toc,
                    toc_open = ad.toc_open,
                    visibility = ad.visibility
                FROM public.tutorial_article_draft ad
                WHERE ad.id = a.current_draft_id;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_folder",
                schema: "public",
                table: "tutorial_article",
                column: "folder");

            migrationBuilder.AddForeignKey(
                name: "fk_tutorial_article_tutorial_folders_folder",
                schema: "public",
                table: "tutorial_article",
                column: "folder",
                principalSchema: "public",
                principalTable: "tutorial_folder",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "fk_tutorial_article_tutorial_article_drafts_current_draft_id",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropTable(
                name: "tutorial_article_draft",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_tutorial_article_current_draft_id",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropIndex(
                name: "ix_tutorial_article_trashed_at",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "current_draft_id",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "tutorial_article");
        }
    }
}
