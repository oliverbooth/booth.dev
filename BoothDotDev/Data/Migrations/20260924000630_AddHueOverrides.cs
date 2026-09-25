using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHueOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<PaletteHue>(
                name: "color",
                schema: "public",
                table: "tutorial_folder",
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
                table: "blog_post",
                type: "public.palette_hue",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "tutorial_folder");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropColumn(
                name: "color",
                schema: "public",
                table: "blog_post");
        }
    }
}
