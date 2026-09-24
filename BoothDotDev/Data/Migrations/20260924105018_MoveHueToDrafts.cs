using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveHueToDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // colour moves from the parent row onto each draft, so a revision remembers its own colour. the copy runs
            // before the drop, and only the current draft receives it: older drafts predate the field and stay null (auto)
            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "blog_post_draft",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "tutorial_article_draft",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "someday_entry_draft",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.blog_post_draft AS d
                SET color = p.color
                FROM public.blog_post AS p
                WHERE p.current_draft_id = d.id AND p.color IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public.tutorial_article_draft AS d
                SET color = p.color
                FROM public.tutorial_article AS p
                WHERE p.current_draft_id = d.id AND p.color IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public.someday_entry_draft AS d
                SET color = p.color
                FROM public.someday_entry AS p
                WHERE p.current_draft_id = d.id AND p.color IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "someday_entry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // the reverse: each parent takes back its current draft's colour; earlier drafts' colours are discarded
            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "blog_post",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "tutorial_article",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "someday_entry",
                type: "public.palette_hue",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public.blog_post AS p
                SET color = d.color
                FROM public.blog_post_draft AS d
                WHERE p.current_draft_id = d.id AND d.color IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public.tutorial_article AS p
                SET color = d.color
                FROM public.tutorial_article_draft AS d
                WHERE p.current_draft_id = d.id AND d.color IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public.someday_entry AS p
                SET color = d.color
                FROM public.someday_entry_draft AS d
                WHERE p.current_draft_id = d.id AND d.color IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "blog_post_draft");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "tutorial_article_draft");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "someday_entry_draft");
        }
    }
}
