using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .Annotation("Npgsql:Enum:public.difficulty", "easy,intermediate,hard,insane")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .Annotation("Npgsql:Enum:public.palette_hue", "brand,grape,bubblegum,tangerine,sun,mint,sky")
                .Annotation("Npgsql:Enum:public.playable_state", "played,playing,plan_to_play")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.streak_cadence_unit", "day,week,month,year")
                .Annotation("Npgsql:Enum:public.streak_check_in_kind", "completed,frozen")
                .Annotation("Npgsql:Enum:public.streak_mode", "check_in,check_out")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .OldAnnotation("Npgsql:Enum:public.difficulty", "easy,intermediate,hard,insane")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,bubblegum,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.streak_cadence_unit", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:public.streak_check_in_kind", "completed,frozen")
                .OldAnnotation("Npgsql:Enum:public.streak_mode", "check_in,check_out")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.CreateTable(
                name: "playable",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    igdb_slug = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    state = table.Column<PlayableState>(type: "public.playable_state", nullable: false),
                    title = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_playable", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_playable_igdb_slug",
                schema: "public",
                table: "playable",
                column: "igdb_slug",
                unique: true,
                filter: "igdb_slug IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "playable",
                schema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .Annotation("Npgsql:Enum:public.difficulty", "easy,intermediate,hard,insane")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .Annotation("Npgsql:Enum:public.palette_hue", "brand,grape,bubblegum,tangerine,sun,mint,sky")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.streak_cadence_unit", "day,week,month,year")
                .Annotation("Npgsql:Enum:public.streak_check_in_kind", "completed,frozen")
                .Annotation("Npgsql:Enum:public.streak_mode", "check_in,check_out")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .OldAnnotation("Npgsql:Enum:public.difficulty", "easy,intermediate,hard,insane")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,bubblegum,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.playable_state", "played,playing,plan_to_play")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.streak_cadence_unit", "day,week,month,year")
                .OldAnnotation("Npgsql:Enum:public.streak_check_in_kind", "completed,frozen")
                .OldAnnotation("Npgsql:Enum:public.streak_mode", "check_in,check_out")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");
        }
    }
}
