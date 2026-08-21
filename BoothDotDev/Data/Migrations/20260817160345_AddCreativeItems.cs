using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreativeItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "artwork_item",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resolution = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false, defaultValue: Visibility.Published),
                    is_work_in_progress = table.Column<bool>(type: "boolean", nullable: false),
                    made_with = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artwork_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "music_item",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false, defaultValue: Visibility.Published),
                    is_work_in_progress = table.Column<bool>(type: "boolean", nullable: false),
                    made_with = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_music_item", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "artwork_item",
                schema: "public");

            migrationBuilder.DropTable(
                name: "music_item",
                schema: "public");
        }
    }
}
