using System;
using System.Collections.Generic;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitBlogPostIntoDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Phase 1: expand. Add the new table and pointer column; nothing existing is touched yet. ---

            migrationBuilder.CreateTable(
                name: "blog_post_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    blog_post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    excerpt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    show_toc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    toc_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_post_draft_blog_post_blog_post_id",
                        column: x => x.blog_post_id,
                        principalSchema: "public",
                        principalTable: "blog_post",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blog_post_draft_blog_post_category_category_id",
                        column: x => x.category_id,
                        principalSchema: "public",
                        principalTable: "blog_post_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_draft_blog_post_id",
                schema: "public",
                table: "blog_post_draft",
                column: "blog_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_draft_category_id",
                schema: "public",
                table: "blog_post_draft",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_draft_created_at",
                schema: "public",
                table: "blog_post_draft",
                column: "created_at");

            migrationBuilder.AddColumn<Guid>(
                name: "current_draft_id",
                schema: "public",
                table: "blog_post",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_current_draft_id",
                schema: "public",
                table: "blog_post",
                column: "current_draft_id");

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_blog_post_drafts_current_draft_id",
                schema: "public",
                table: "blog_post",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "blog_post_draft",
                principalColumn: "id");

            // --- Phase 2: migrate. Give every existing post a generation-zero draft carrying its current
            // content, then point it at that draft. Nothing here is hand-typed - Postgres copies every row
            // in one pass. `password` is deliberately not copied anywhere; it's being removed outright. ---

            migrationBuilder.Sql(
                """
                INSERT INTO public.blog_post_draft (id, blog_post_id, title, body, excerpt, tags, category_id, visibility, show_toc, toc_open, created_at)
                SELECT gen_random_uuid(), id, title, body, excerpt, tags, category_id, visibility, show_toc, toc_open, COALESCE(updated, published)
                FROM public.blog_post;

                UPDATE public.blog_post bp
                SET current_draft_id = bpd.id
                FROM public.blog_post_draft bpd
                WHERE bpd.blog_post_id = bp.id;
                """);

            // --- Phase 3: contract. Every post now has a current draft holding its old content, so the
            // columns that content used to live in on `blog_post` are safe to drop. ---

            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_blog_post_category_category_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropIndex(
                name: "ix_blog_post_category_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "body",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "category_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "excerpt",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "password",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "show_toc",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "tags",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "title",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "toc_open",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "visibility",
                schema: "public",
                table: "blog_post");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Reverse of phase 3: bring the old columns back (password can't be restored - it was
            // deliberately removed, not just relocated). ---

            migrationBuilder.AddColumn<string>(
                name: "body",
                schema: "public",
                table: "blog_post",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Left nullable rather than backfilled with a placeholder Guid: a placeholder value would
            // immediately violate the FK to blog_post_category re-added below. The real values get restored
            // from the pre-migration backup in the same operation that runs this rollback.
            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                schema: "public",
                table: "blog_post",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "excerpt",
                schema: "public",
                table: "blog_post",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                schema: "public",
                table: "blog_post",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "show_toc",
                schema: "public",
                table: "blog_post",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "tags",
                schema: "public",
                table: "blog_post",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "public",
                table: "blog_post",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "toc_open",
                schema: "public",
                table: "blog_post",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "blog_post",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Private);

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_category_id",
                schema: "public",
                table: "blog_post",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_blog_post_category_category_id",
                schema: "public",
                table: "blog_post",
                column: "category_id",
                principalSchema: "public",
                principalTable: "blog_post_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            // Backfill the resurrected columns from each post's current draft, while both `current_draft_id`
            // and `blog_post_draft` still exist. This only restores the *current* draft's content - the rest
            // of the draft history has nowhere to go in the old schema and is discarded here.

            migrationBuilder.Sql(
                """
                UPDATE public.blog_post bp
                SET title = bpd.title,
                    body = bpd.body,
                    excerpt = bpd.excerpt,
                    tags = bpd.tags,
                    category_id = bpd.category_id,
                    visibility = bpd.visibility,
                    show_toc = bpd.show_toc,
                    toc_open = bpd.toc_open
                FROM public.blog_post_draft bpd
                WHERE bpd.id = bp.current_draft_id;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_blog_post_drafts_current_draft_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropTable(
                name: "blog_post_draft",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_blog_post_current_draft_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "current_draft_id",
                schema: "public",
                table: "blog_post");
        }
    }
}
