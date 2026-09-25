using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeCreations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // artwork_item and music_item are deliberately left in place: the data is copied into creation below, and the old
            // tables are dropped by a later migration once this one has proven out in production

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
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.CreateTable(
                name: "creation",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<CreationKind>(type: "public.creation_kind", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    trashed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false, defaultValue: Visibility.Published),
                    is_work_in_progress = table.Column<bool>(type: "boolean", nullable: false),
                    made_with = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    resolution = table.Column<string>(type: "text", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_creation", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_creation_trashed_at",
                schema: "public",
                table: "creation",
                column: "trashed_at");

            // nothing recorded whether artwork was a render or a drawing, so Blender, the only 3D tool used so far, decides
            migrationBuilder.Sql("""
                INSERT INTO public.creation (id, kind, file_name, title, description, published_at, trashed_at, visibility,
                                             is_work_in_progress, made_with, resolution, duration)
                SELECT id,
                       CASE WHEN made_with ILIKE '%blender%' THEN 'three_d'::public.creation_kind
                            ELSE 'drawing'::public.creation_kind END,
                       file_name, title, description, published_at, trashed_at, visibility,
                       is_work_in_progress, made_with, resolution, NULL
                FROM public.artwork_item;

                INSERT INTO public.creation (id, kind, file_name, title, description, published_at, trashed_at, visibility,
                                             is_work_in_progress, made_with, resolution, duration)
                SELECT id, 'music'::public.creation_kind, file_name, title, description, published_at, trashed_at, visibility,
                       is_work_in_progress, made_with, NULL, duration
                FROM public.music_item;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // the old tables were never dropped, so they still hold everything from before this migration. Creations added
            // since then are lost
            migrationBuilder.DropTable(
                name: "creation",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
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
