using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePinkToBubblegum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF only diffs the enum's label list, so it can't tell a rename from a removal plus an addition and would not
            // rename the label in place; every row's value follows the renamed label, so nothing is rewritten
            migrationBuilder.Sql("ALTER TYPE public.palette_hue RENAME VALUE 'pink' TO 'bubblegum';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TYPE public.palette_hue RENAME VALUE 'bubblegum' TO 'pink';");
        }
    }
}
