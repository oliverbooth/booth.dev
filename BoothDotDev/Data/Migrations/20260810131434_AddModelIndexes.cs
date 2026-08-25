using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModelIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_tutorial_folder_parent",
                schema: "public",
                table: "tutorial_folder",
                column: "parent");

            migrationBuilder.CreateIndex(
                name: "ix_tutorial_article_folder",
                schema: "public",
                table: "tutorial_article",
                column: "folder");

            migrationBuilder.CreateIndex(
                name: "ix_project_slug",
                schema: "public",
                table: "project",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_author_id",
                schema: "public",
                table: "blog_post",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_category_id",
                schema: "public",
                table: "blog_post",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_slug",
                schema: "public",
                table: "blog_post",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_blog_post_category_category_id",
                schema: "public",
                table: "blog_post",
                column: "category_id",
                principalSchema: "public",
                principalTable: "blog_post_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_users_author_id",
                schema: "public",
                table: "blog_post",
                column: "author_id",
                principalSchema: "public",
                principalTable: "user",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tutorial_article_tutorial_folders_folder",
                schema: "public",
                table: "tutorial_article",
                column: "folder",
                principalSchema: "public",
                principalTable: "tutorial_folder",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_tutorial_folder_tutorial_folder_parent",
                schema: "public",
                table: "tutorial_folder",
                column: "parent",
                principalSchema: "public",
                principalTable: "tutorial_folder",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_blog_post_category_category_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_users_author_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropForeignKey(
                name: "fk_tutorial_article_tutorial_folders_folder",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropForeignKey(
                name: "fk_tutorial_folder_tutorial_folder_parent",
                schema: "public",
                table: "tutorial_folder");

            migrationBuilder.DropIndex(
                name: "ix_tutorial_folder_parent",
                schema: "public",
                table: "tutorial_folder");

            migrationBuilder.DropIndex(
                name: "ix_tutorial_article_folder",
                schema: "public",
                table: "tutorial_article");

            migrationBuilder.DropIndex(
                name: "ix_project_slug",
                schema: "public",
                table: "project");

            migrationBuilder.DropIndex(
                name: "ix_blog_post_author_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropIndex(
                name: "ix_blog_post_category_id",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropIndex(
                name: "ix_blog_post_slug",
                schema: "public",
                table: "blog_post");
        }
    }
}
