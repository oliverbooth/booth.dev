using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    alt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                    table.CheckConstraint("ck_media_one_owner", "(project_id IS NULL) <> (creation_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_media_creation_creation_id",
                        column: x => x.creation_id,
                        principalSchema: "public",
                        principalTable: "creation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_media_project_project_id",
                        column: x => x.project_id,
                        principalSchema: "public",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_creation_id_file_name",
                schema: "public",
                table: "media",
                columns: new[] { "creation_id", "file_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_media_project_id_file_name",
                schema: "public",
                table: "media",
                columns: new[] { "project_id", "file_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_media_creation_cover",
                schema: "public",
                table: "media",
                column: "creation_id",
                unique: true,
                filter: "is_cover");

            migrationBuilder.CreateIndex(
                name: "ux_media_project_cover",
                schema: "public",
                table: "media",
                column: "project_id",
                unique: true,
                filter: "is_cover");

            // projects with no image point at a real placeholder file named none-*, which is not a cover
            migrationBuilder.Sql("""
                INSERT INTO public.media (id, project_id, creation_id, file_name, "position", is_cover, alt, width, height, duration)
                SELECT gen_random_uuid(), p.id, NULL, p.hero_url, 0,
                       p.hero_url ~* '\.(png|jpe?g|gif|webp|svg)$',
                       NULL, NULL, NULL, NULL
                FROM public.project p
                WHERE p.hero_url <> '' AND p.hero_url NOT LIKE 'none-%';

                INSERT INTO public.media (id, project_id, creation_id, file_name, "position", is_cover, alt, width, height, duration)
                SELECT gen_random_uuid(), NULL, c.id, c.file_name, 0,
                       c.file_name ~* '\.(png|jpe?g|gif|webp|svg)$',
                       NULL,
                       CASE WHEN c.resolution ~ '^\d+x\d+$' THEN split_part(c.resolution, 'x', 1)::int END,
                       CASE WHEN c.resolution ~ '^\d+x\d+$' THEN split_part(c.resolution, 'x', 2)::int END,
                       c.duration
                FROM public.creation c
                WHERE c.file_name <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media",
                schema: "public");
        }
    }
}
