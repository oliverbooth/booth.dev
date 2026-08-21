using BoothDotDev.Data.Configuration;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data;

/// <summary>
///     Represents a session with the application database.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AppDbContext" /> class.
    /// </summary>
    /// <param name="options">The options for creating a new context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    ///     Gets the collection of artwork items in the database.
    /// </summary>
    /// <value>The collection of artwork items.</value>
    public DbSet<ArtworkItem> ArtworkItems
    {
        get => Set<ArtworkItem>();
    }

    /// <summary>
    ///     Gets the collection of blog post categories in the database.
    /// </summary>
    /// <value>The collection of blog post categories.</value>
    public DbSet<BlogPostCategory> BlogPostCategories
    {
        get => Set<BlogPostCategory>();
    }

    /// <summary>
    ///     Gets the collection of blog post drafts in the database.
    /// </summary>
    /// <value>The collection of blog post drafts.</value>
    public DbSet<BlogPostDraft> BlogPostDrafts
    {
        get => Set<BlogPostDraft>();
    }

    /// <summary>
    ///     Gets the collection of blog posts in the database.
    /// </summary>
    /// <value>The collection of blog posts.</value>
    public DbSet<BlogPost> BlogPosts
    {
        get => Set<BlogPost>();
    }

    /// <summary>
    ///     Gets the collection of books in the reading list.
    /// </summary>
    /// <value>The collection of books.</value>
    public DbSet<Book> Books
    {
        get => Set<Book>();
    }

    /// <summary>
    ///     Gets the collection of code snippets in the database.
    /// </summary>
    /// <value>The collection of code snippets.</value>
    public DbSet<CodeSnippet> CodeSnippets
    {
        get => Set<CodeSnippet>();
    }

    /// <summary>
    ///     Gets the collection of dev challenges in the database.
    /// </summary>
    /// <value>The collection of dev challenges.</value>
    public DbSet<DevChallenge> DevChallenges
    {
        get => Set<DevChallenge>();
    }

    /// <summary>
    ///     Gets the collection of project devlogs in the database.
    /// </summary>
    /// <value>The collection of project devlogs.</value>
    public DbSet<ProjectDevlog> DevLogs
    {
        get => Set<ProjectDevlog>();
    }

    /// <summary>
    ///     Gets the collection of legacy comments in the database.
    /// </summary>
    /// <value>The collection of legacy comments.</value>
    public DbSet<LegacyComment> LegacyComments
    {
        get => Set<LegacyComment>();
    }

    /// <summary>
    ///     Gets the collection of music items in the database.
    /// </summary>
    /// <value>The collection of music items.</value>
    public DbSet<MusicItem> MusicItems
    {
        get => Set<MusicItem>();
    }

    /// <summary>
    ///     Gets the collection of note drafts in the database.
    /// </summary>
    /// <value>The collection of note drafts.</value>
    public DbSet<NoteDraft> NoteDrafts
    {
        get => Set<NoteDraft>();
    }

    /// <summary>
    ///     Gets the collection of notes in the database.
    /// </summary>
    /// <value>The collection of notes.</value>
    public DbSet<Note> Notes
    {
        get => Set<Note>();
    }

    /// <summary>
    ///     Gets the collection of projects in the database.
    /// </summary>
    /// <value>The collection of projects.</value>
    public DbSet<Project> Projects
    {
        get => Set<Project>();
    }

    /// <summary>
    ///     Gets the set of site configuration items.
    /// </summary>
    /// <value>The set of site configuration items.</value>
    public DbSet<SiteConfiguration> SiteConfiguration
    {
        get => Set<SiteConfiguration>();
    }

    /// <summary>
    ///     Gets the collection of tutorial articles in the database.
    /// </summary>
    /// <value>The collection of tutorial articles.</value>
    public DbSet<TutorialArticle> TutorialArticles
    {
        get => Set<TutorialArticle>();
    }

    /// <summary>
    ///     Gets the collection of tutorial folders in the database.
    /// </summary>
    /// <value>The collection of tutorial folders.</value>
    public DbSet<TutorialFolder> TutorialFolders
    {
        get => Set<TutorialFolder>();
    }

    /// <summary>
    ///     Gets the collection of users in the database.
    /// </summary>
    /// <value>The collection of users.</value>
    public DbSet<User> Users
    {
        get => Set<User>();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasPostgresEnum<BookState>("public", "book_state", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<FontStyle>("public", "font_style", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<ProjectStatus>("public", "project_status", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<ProjectType>("public", "project_type", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<Visibility>("public", "visibility", new NpgsqlSnakeCaseNameTranslator());

        modelBuilder.ApplyConfiguration(new ArtworkItemConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostDraftConfiguration());
        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new CodeSnippetConfiguration());
        modelBuilder.ApplyConfiguration(new DevChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new LegacyCommentConfiguration());
        modelBuilder.ApplyConfiguration(new MusicItemConfiguration());
        modelBuilder.ApplyConfiguration(new NoteConfiguration());
        modelBuilder.ApplyConfiguration(new NoteDraftConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectDevlogConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialArticleConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialFolderConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
