using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePublishedAndUpdatedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated",
                schema: "public",
                table: "tutorial_article",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "tutorial_article",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "updated",
                schema: "public",
                table: "note",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "note",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "music_item",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "updated",
                schema: "public",
                table: "devlog",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "devlog",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "updated",
                schema: "public",
                table: "dev_challenge",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "updated",
                schema: "public",
                table: "blog_post",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "blog_post",
                newName: "published_at");

            migrationBuilder.RenameColumn(
                name: "published",
                schema: "public",
                table: "artwork_item",
                newName: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "tutorial_article",
                newName: "updated");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "tutorial_article",
                newName: "published");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "note",
                newName: "updated");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "note",
                newName: "published");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "music_item",
                newName: "published");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "devlog",
                newName: "updated");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "devlog",
                newName: "published");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "dev_challenge",
                newName: "updated");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "public",
                table: "blog_post",
                newName: "updated");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "blog_post",
                newName: "published");

            migrationBuilder.RenameColumn(
                name: "published_at",
                schema: "public",
                table: "artwork_item",
                newName: "published");
        }
    }
}
