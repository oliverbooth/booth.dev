using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSomedayEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "someday_entry",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    trashed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_someday_entry", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "someday_entry_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    someday_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_someday_entry_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_someday_entry_draft_someday_entry_someday_entry_id",
                        column: x => x.someday_entry_id,
                        principalSchema: "public",
                        principalTable: "someday_entry",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_current_draft_id",
                schema: "public",
                table: "someday_entry",
                column: "current_draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_slug",
                schema: "public",
                table: "someday_entry",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_sort_order",
                schema: "public",
                table: "someday_entry",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_trashed_at",
                schema: "public",
                table: "someday_entry",
                column: "trashed_at");

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_draft_created_at",
                schema: "public",
                table: "someday_entry_draft",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_someday_entry_draft_someday_entry_id",
                schema: "public",
                table: "someday_entry_draft",
                column: "someday_entry_id");

            migrationBuilder.AddForeignKey(
                name: "fk_someday_entry_someday_entry_drafts_current_draft_id",
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
                name: "fk_someday_entry_someday_entry_drafts_current_draft_id",
                schema: "public",
                table: "someday_entry");

            migrationBuilder.DropTable(
                name: "someday_entry_draft",
                schema: "public");

            migrationBuilder.DropTable(
                name: "someday_entry",
                schema: "public");
        }
    }
}
