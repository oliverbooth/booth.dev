using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "tags",
                schema: "public",
                table: "project",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "tags",
                schema: "public",
                table: "creation",
                type: "text[]",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE public.project SET tags = '{}';
                UPDATE public.creation SET tags = '{}';
                """);

            migrationBuilder.AlterColumn<List<string>>(
                name: "tags",
                schema: "public",
                table: "project",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.AlterColumn<List<string>>(
                name: "tags",
                schema: "public",
                table: "creation",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tags",
                schema: "public",
                table: "project");

            migrationBuilder.DropColumn(
                name: "tags",
                schema: "public",
                table: "creation");
        }
    }
}
