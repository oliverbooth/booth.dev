using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPostTrash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "trashed_at",
                schema: "public",
                table: "blog_post",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_trashed_at",
                schema: "public",
                table: "blog_post",
                column: "trashed_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_blog_post_trashed_at",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "trashed_at",
                schema: "public",
                table: "blog_post");
        }
    }
}
