using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayableEditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "playable_edition",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    igdb_slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    label = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    platforms = table.Column<string[]>(type: "text[]", nullable: false),
                    playable_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_playable_edition", x => x.id);
                    table.ForeignKey(
                        name: "fk_playable_edition_playable_playable_id",
                        column: x => x.playable_id,
                        principalSchema: "public",
                        principalTable: "playable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_playable_edition_playable_id",
                schema: "public",
                table: "playable_edition",
                column: "playable_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playable_edition",
                schema: "public");
        }
    }
}
