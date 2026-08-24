using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDevChallengeExcerpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "excerpt",
                schema: "public",
                table: "dev_challenge_draft",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "excerpt",
                schema: "public",
                table: "dev_challenge_draft");
        }
    }
}
