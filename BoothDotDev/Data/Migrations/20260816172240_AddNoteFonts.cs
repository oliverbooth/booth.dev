using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteFonts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<FontStyle>(
                name: "font_style",
                schema: "public",
                table: "note",
                type: "public.font_style",
                nullable: false,
                defaultValue: FontStyle.Serif);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "font_style",
                schema: "public",
                table: "note");
        }
    }
}
