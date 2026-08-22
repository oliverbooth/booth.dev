using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreativeItemTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "music_item",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Private,
                oldClrType: typeof(Visibility),
                oldType: "public.visibility",
                oldDefaultValue: Visibility.Published);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "music_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "artwork_item",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_music_item_trashed_at",
                schema: "public",
                table: "music_item",
                column: "trashed_at");

            migrationBuilder.CreateIndex(
                name: "ix_artwork_item_trashed_at",
                schema: "public",
                table: "artwork_item",
                column: "trashed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_music_item_trashed_at",
                schema: "public",
                table: "music_item");

            migrationBuilder.DropIndex(
                name: "ix_artwork_item_trashed_at",
                schema: "public",
                table: "artwork_item");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "music_item");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "artwork_item");

            migrationBuilder.AlterColumn<Visibility>(
                name: "visibility",
                schema: "public",
                table: "music_item",
                type: "public.visibility",
                nullable: false,
                defaultValue: Visibility.Published,
                oldClrType: typeof(Visibility),
                oldType: "public.visibility",
                oldDefaultValue: Visibility.Private);
        }
    }
}
