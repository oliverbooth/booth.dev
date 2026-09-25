using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoothDotDev.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixDraftForeignKeyNames : Migration
    {
        private static void RenameConstraint(MigrationBuilder migrationBuilder, string table, string from, string to)
        {
            migrationBuilder.Sql($"""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM pg_constraint WHERE conname = '{from}') THEN
                        ALTER TABLE public.{table} RENAME CONSTRAINT {from} TO {to};
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Guarded rather than a blind DropForeignKey/AddForeignKey pair: "note" was already renamed on some
            // databases (including local dev) outside of a migration, so a blind drop fails there with "constraint
            // ... does not exist". A guarded rename is a no-op wherever the target name is already in place.
            RenameConstraint(migrationBuilder, "blog_post", "fk_blog_post_blog_post_drafts_current_draft_id", "fk_blog_post_blog_post_draft_current_draft_id");
            RenameConstraint(migrationBuilder, "dev_challenge", "fk_dev_challenge_dev_challenge_drafts_current_draft_id", "fk_dev_challenge_dev_challenge_draft_current_draft_id");
            RenameConstraint(migrationBuilder, "devlog", "fk_devlog_project_devlog_drafts_current_draft_id", "fk_devlog_devlog_draft_current_draft_id");
            RenameConstraint(migrationBuilder, "note", "fk_note_note_drafts_current_draft_id", "fk_note_note_draft_current_draft_id");
            RenameConstraint(migrationBuilder, "tutorial_article", "fk_tutorial_article_tutorial_article_drafts_current_draft_id", "fk_tutorial_article_tutorial_article_draft_current_draft_id");
            RenameConstraint(migrationBuilder, "tutorial_article_draft", "fk_tutorial_article_draft_tutorial_folders_folder", "fk_tutorial_article_draft_tutorial_folder_folder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RenameConstraint(migrationBuilder, "blog_post", "fk_blog_post_blog_post_draft_current_draft_id", "fk_blog_post_blog_post_drafts_current_draft_id");
            RenameConstraint(migrationBuilder, "dev_challenge", "fk_dev_challenge_dev_challenge_draft_current_draft_id", "fk_dev_challenge_dev_challenge_drafts_current_draft_id");
            RenameConstraint(migrationBuilder, "devlog", "fk_devlog_devlog_draft_current_draft_id", "fk_devlog_project_devlog_drafts_current_draft_id");
            RenameConstraint(migrationBuilder, "note", "fk_note_note_draft_current_draft_id", "fk_note_note_drafts_current_draft_id");
            RenameConstraint(migrationBuilder, "tutorial_article", "fk_tutorial_article_tutorial_article_draft_current_draft_id", "fk_tutorial_article_tutorial_article_drafts_current_draft_id");
            RenameConstraint(migrationBuilder, "tutorial_article_draft", "fk_tutorial_article_draft_tutorial_folder_folder", "fk_tutorial_article_draft_tutorial_folders_folder");
        }
    }
}
