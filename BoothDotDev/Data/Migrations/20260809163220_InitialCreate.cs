using System;
using System.Collections.Generic;
using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.book_state", "read,reading,plan_to_read")
                .Annotation("Npgsql:Enum:public.font_style", "sans_serif,serif")
                .Annotation("Npgsql:Enum:public.project_status", "ongoing,hiatus,past,retired")
                .Annotation("Npgsql:Enum:public.project_type", "app,game,library,tool,website")
                .Annotation("Npgsql:Enum:public.visibility", "none,private,unlisted,published");

            migrationBuilder.CreateTable(
                name: "blog_post",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enable_comments = table.Column<bool>(type: "boolean", nullable: false),
                    excerpt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_redirect = table.Column<bool>(type: "boolean", nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    redirect_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    show_toc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    toc_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false),
                    wordpress_id = table.Column<int>(type: "integer", nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    disqus_domain = table.Column<string>(type: "text", nullable: true),
                    disqus_identifier = table.Column<string>(type: "text", nullable: true),
                    disqus_path = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog_post_category",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    font_style = table.Column<FontStyle>(type: "public.font_style", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_category", x => x.id);
                    table.ForeignKey(
                        name: "fk_blog_post_category_blog_post_category_parent_category_id",
                        column: x => x.parent_category_id,
                        principalSchema: "public",
                        principalTable: "blog_post_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "book",
                schema: "public",
                columns: table => new
                {
                    isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    author = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    state = table.Column<BookState>(type: "public.book_state", nullable: false),
                    title = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_book", x => x.isbn);
                });

            migrationBuilder.CreateTable(
                name: "code_snippet",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_code_snippet", x => new { x.id, x.language });
                });

            migrationBuilder.CreateTable(
                name: "dev_challenge",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    old_id = table.Column<int>(type: "integer", nullable: true),
                    password = table.Column<string>(type: "text", nullable: true),
                    show_solution = table.Column<bool>(type: "boolean", nullable: false),
                    solution = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dev_challenge", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "legacy_comment",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    avatar = table.Column<string>(type: "character varying(32767)", maxLength: 32767, nullable: true),
                    author = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    body = table.Column<string>(type: "character varying(32767)", maxLength: 32767, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    parent_comment = table.Column<Guid>(type: "uuid", nullable: true),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legacy_comment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "project",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "text", nullable: false),
                    hero_url = table.Column<string>(type: "text", nullable: false),
                    languages = table.Column<List<string>>(type: "text[]", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    remote_target = table.Column<string>(type: "text", nullable: true),
                    remote_url = table.Column<string>(type: "text", nullable: true),
                    slug = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<ProjectStatus>(type: "public.project_status", nullable: false),
                    tagline = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<ProjectType>(type: "public.project_type", nullable: false, defaultValue: ProjectType.App)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site_config",
                schema: "public",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    value = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site_config", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "template",
                schema: "public",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    variant = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    format_string = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template", x => new { x.name, x.variant });
                });

            migrationBuilder.CreateTable(
                name: "tutorial_article",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    enable_comments = table.Column<bool>(type: "boolean", nullable: false),
                    excerpt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    folder = table.Column<Guid>(type: "uuid", nullable: false),
                    next_part = table.Column<Guid>(type: "uuid", nullable: true),
                    preview_image_url = table.Column<string>(type: "text", nullable: true),
                    previous_part = table.Column<Guid>(type: "uuid", nullable: true),
                    published = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    redirect_from = table.Column<Guid>(type: "uuid", nullable: true),
                    show_toc = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    slug = table.Column<string>(type: "text", nullable: false),
                    toc_open = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tutorial_article", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tutorial_folder",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    parent = table.Column<Guid>(type: "uuid", nullable: true),
                    preview_image_url = table.Column<string>(type: "text", nullable: true),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    visibility = table.Column<Visibility>(type: "public.visibility", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tutorial_folder", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    registered = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    salt = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_category_parent_category_id",
                schema: "public",
                table: "blog_post_category",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_category_slug",
                schema: "public",
                table: "blog_post_category",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_post",
                schema: "public");

            migrationBuilder.DropTable(
                name: "blog_post_category",
                schema: "public");

            migrationBuilder.DropTable(
                name: "book",
                schema: "public");

            migrationBuilder.DropTable(
                name: "code_snippet",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dev_challenge",
                schema: "public");

            migrationBuilder.DropTable(
                name: "legacy_comment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "project",
                schema: "public");

            migrationBuilder.DropTable(
                name: "site_config",
                schema: "public");

            migrationBuilder.DropTable(
                name: "template",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tutorial_article",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tutorial_folder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user",
                schema: "public");
        }
    }
}
