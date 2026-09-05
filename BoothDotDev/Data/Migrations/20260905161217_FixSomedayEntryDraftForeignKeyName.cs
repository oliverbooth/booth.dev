using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSomedayEntryDraftForeignKeyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_someday_entry_someday_entry_drafts_current_draft_id",
                schema: "public",
                table: "someday_entry");

            migrationBuilder.AddForeignKey(
                name: "fk_someday_entry_someday_entry_draft_current_draft_id",
                schema: "public",
                table: "someday_entry",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "someday_entry_draft",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_someday_entry_someday_entry_draft_current_draft_id",
                schema: "public",
                table: "someday_entry");

            migrationBuilder.AddForeignKey(
                name: "fk_someday_entry_someday_entry_drafts_current_draft_id",
                schema: "public",
                table: "someday_entry",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "someday_entry_draft",
                principalColumn: "id");
        }
    }
}
