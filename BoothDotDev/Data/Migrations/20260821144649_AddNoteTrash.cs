using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "note",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_note_trashed_at",
                schema: "public",
                table: "note",
                column: "trashed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_note_trashed_at",
                schema: "public",
                table: "note");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "note");
        }
    }
}
