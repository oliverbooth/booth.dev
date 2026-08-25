using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDisqus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "disqus_domain",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "disqus_identifier",
                schema: "public",
                table: "blog_post");

            migrationBuilder.DropColumn(
                name: "disqus_path",
                schema: "public",
                table: "blog_post");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "disqus_domain",
                schema: "public",
                table: "blog_post",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "disqus_identifier",
                schema: "public",
                table: "blog_post",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "disqus_path",
                schema: "public",
                table: "blog_post",
                type: "text",
                nullable: true);
        }
    }
}
