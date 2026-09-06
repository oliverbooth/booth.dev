using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTraktSync : Migration
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
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.AddColumn<WatchableSource>(
                name: "source",
                schema: "public",
                table: "watchable",
                type: "public.watchable_source",
                nullable: false,
                defaultValue: WatchableSource.Manual);

            migrationBuilder.AddColumn<int>(
                name: "trakt_id",
                schema: "public",
                table: "watchable",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "trakt_credential",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    access_token = table.Column<string>(type: "text", nullable: false),
                    refresh_token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trakt_credential", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_watchable_trakt_id_kind",
                schema: "public",
                table: "watchable",
                columns: new[] { "trakt_id", "kind" },
                unique: true,
                filter: "trakt_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trakt_credential",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "ix_watchable_trakt_id_kind",
                schema: "public",
                table: "watchable");

            migrationBuilder.DropColumn(
                name: "source",
                schema: "public",
                table: "watchable");

            migrationBuilder.DropColumn(
                name: "trakt_id",
                schema: "public",
                table: "watchable");

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
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");
        }
    }
}
