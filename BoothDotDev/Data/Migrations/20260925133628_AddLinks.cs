using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other")
                .Annotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.CreateTable(
                name: "link",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<LinkKind>(type: "public.link_kind", nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_link", x => x.id);
                    table.CheckConstraint("ck_link_one_owner", "(project_id IS NULL) <> (creation_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_link_creation_creation_id",
                        column: x => x.creation_id,
                        principalSchema: "public",
                        principalTable: "creation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_link_project_project_id",
                        column: x => x.project_id,
                        principalSchema: "public",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_link_creation_primary",
                schema: "public",
                table: "link",
                column: "creation_id",
                unique: true,
                filter: "is_primary");

            migrationBuilder.CreateIndex(
                name: "ux_link_project_primary",
                schema: "public",
                table: "link",
                column: "project_id",
                unique: true,
                filter: "is_primary");

            migrationBuilder.Sql("""
                INSERT INTO public.link (id, project_id, creation_id, kind, url, label, "position", is_primary)
                SELECT gen_random_uuid(), p.id, NULL,
                       CASE p.remote_target
                            WHEN 'GitHub' THEN 'github'
                            WHEN 'GitLab' THEN 'gitlab'
                            WHEN 'Itch.io' THEN 'itch'
                            WHEN 'PlayStore' THEN 'play_store'
                            ELSE 'website'
                       END::public.link_kind,
                       p.remote_url, NULL, 0, true
                FROM public.project p
                WHERE p.remote_url IS NOT NULL AND p.remote_url <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "link",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");
        }
    }
}
