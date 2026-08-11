using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "template",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "template",
                schema: "public",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    variant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    format_string = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template", x => new { x.name, x.variant });
                });
        }
    }
}
