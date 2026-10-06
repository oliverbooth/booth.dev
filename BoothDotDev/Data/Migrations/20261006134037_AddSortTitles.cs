using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSortTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sort_title",
                schema: "public",
                table: "watchable",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sort_title",
                schema: "public",
                table: "playable",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sort_title",
                schema: "public",
                table: "book",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort_title",
                schema: "public",
                table: "watchable");

            migrationBuilder.DropColumn(
                name: "sort_title",
                schema: "public",
                table: "playable");

            migrationBuilder.DropColumn(
                name: "sort_title",
                schema: "public",
                table: "book");
        }
    }
}
