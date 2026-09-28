using System;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStreaks : Migration
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
                .OldAnnotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published")
                .OldAnnotation("Npgsql:Enum:public.watchable_kind", "movie,show")
                .OldAnnotation("Npgsql:Enum:public.watchable_source", "manual,trakt")
                .OldAnnotation("Npgsql:Enum:public.watchable_state", "watched,watching,plan_to_watch");

            migrationBuilder.CreateTable(
                name: "streak",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_draft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    started_on = table.Column<DateOnly>(type: "date", nullable: true),
                    trashed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streak", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "streak_check_in",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    streak_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on = table.Column<DateOnly>(type: "date", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streak_check_in", x => x.id);
                    table.ForeignKey(
                        name: "fk_streak_check_in_streak_streak_id",
                        column: x => x.streak_id,
                        principalSchema: "public",
                        principalTable: "streak",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "streak_draft",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cadence_interval = table.Column<int>(type: "integer", nullable: true),
                    cadence_unit = table.Column<int>(type: "integer", nullable: true),
                    color = table.Column<PaletteHue>(type: "public.palette_hue", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    dot_color = table.Column<PaletteHue>(type: "public.palette_hue", nullable: true),
                    streak_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false),
                    body = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streak_draft", x => x.id);
                    table.ForeignKey(
                        name: "fk_streak_draft_streak_streak_id",
                        column: x => x.streak_id,
                        principalSchema: "public",
                        principalTable: "streak",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "streak_reset",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    streak_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_on = table.Column<DateOnly>(type: "date", nullable: false),
                    note = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streak_reset", x => x.id);
                    table.ForeignKey(
                        name: "fk_streak_reset_streak_streak_id",
                        column: x => x.streak_id,
                        principalSchema: "public",
                        principalTable: "streak",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_streak_current_draft_id",
                schema: "public",
                table: "streak",
                column: "current_draft_id");

            migrationBuilder.CreateIndex(
                name: "ix_streak_slug",
                schema: "public",
                table: "streak",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_streak_sort_order",
                schema: "public",
                table: "streak",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "ix_streak_trashed_at",
                schema: "public",
                table: "streak",
                column: "trashed_at");

            migrationBuilder.CreateIndex(
                name: "ix_streak_check_in_streak_id_occurred_on",
                schema: "public",
                table: "streak_check_in",
                columns: new[] { "streak_id", "occurred_on" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_streak_draft_created_at",
                schema: "public",
                table: "streak_draft",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_streak_draft_streak_id",
                schema: "public",
                table: "streak_draft",
                column: "streak_id");

            migrationBuilder.CreateIndex(
                name: "ix_streak_reset_streak_id_occurred_on",
                schema: "public",
                table: "streak_reset",
                columns: new[] { "streak_id", "occurred_on" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_streak_streak_draft_current_draft_id",
                schema: "public",
                table: "streak",
                column: "current_draft_id",
                principalSchema: "public",
                principalTable: "streak_draft",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_streak_streak_draft_current_draft_id",
                schema: "public",
                table: "streak");

            migrationBuilder.DropTable(
                name: "streak_check_in",
                schema: "public");

            migrationBuilder.DropTable(
                name: "streak_reset",
                schema: "public");

            migrationBuilder.DropTable(
                name: "streak_draft",
                schema: "public");

            migrationBuilder.DropTable(
                name: "streak",
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
        }
    }
}
