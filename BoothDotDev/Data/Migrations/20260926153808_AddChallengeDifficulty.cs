using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeDifficulty : Migration
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
                .OldAnnotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .OldAnnotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .OldAnnotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .OldAnnotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.AddColumn<Difficulty>(
                name: "difficulty",
                schema: "public",
                table: "dev_challenge_draft",
                type: "public.difficulty",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "difficulty",
                schema: "public",
                table: "dev_challenge_draft");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
                .Annotation("Npgsql:Enum:public.palette_hue", "brand,grape,pink,tangerine,sun,mint,sky")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .Annotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .Annotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .Annotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch")
                .OldAnnotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .OldAnnotation("Npgsql:Enum:public.creation_kind", "drawing,three_d,music")
                .OldAnnotation("Npgsql:Enum:public.difficulty", "easy,intermediate,hard,insane")
                .OldAnnotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .OldAnnotation("Npgsql:Enum:public.link_kind", "github,gitlab,itch,play_store,steam,youtube,discord,deviantart,behance,soundcloud,documentation,website,other,gamejolt")
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
