using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published");

            migrationBuilder.CreateTable(
                name: "watchable",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<WatchableKind>(type: "public.watchable_kind", nullable: false),
                    state = table.Column<WatchableState>(type: "public.watchable_state", nullable: false),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_watchable", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "watchable",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");
        }
    }
}
