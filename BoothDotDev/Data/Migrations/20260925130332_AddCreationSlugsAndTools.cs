using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreationSlugsAndTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "public",
                table: "creation",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "tools",
                schema: "public",
                table: "creation",
                type: "text[]",
                nullable: true);

            migrationBuilder.Sql("""
                WITH based AS (
                    SELECT id, published_at,
                           COALESCE(
                               NULLIF(trim(both '-' from regexp_replace(regexp_replace(lower(title), '[^a-z0-9\s_-]', '', 'g'), '[\s_-]+', '-', 'g')), ''),
                               'creation') AS base
                    FROM public.creation
                ),
                ranked AS (
                    SELECT id, base, row_number() OVER (PARTITION BY base ORDER BY published_at, id) AS n
                    FROM based
                )
                UPDATE public.creation c
                SET slug = r.base || CASE WHEN r.n > 1 THEN '-' || r.n ELSE '' END
                FROM ranked r
                WHERE c.id = r.id;

                UPDATE public.creation SET slug = slug || '-creation' WHERE slug IN (SELECT slug FROM public.project);

                UPDATE public.creation
                SET tools = CASE WHEN made_with IS NULL OR made_with = '' THEN '{}' ELSE ARRAY[made_with] END;

                ALTER TABLE public.creation ALTER COLUMN slug DROP DEFAULT;
                """);

            migrationBuilder.AlterColumn<List<string>>(
                name: "tools",
                schema: "public",
                table: "creation",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_creation_slug",
                schema: "public",
                table: "creation",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_creation_slug",
                schema: "public",
                table: "creation");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "public",
                table: "creation");

            migrationBuilder.DropColumn(
                name: "tools",
                schema: "public",
                table: "creation");
        }
    }
}
