using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Models;
using BoothDotDev.Data.Configuration;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data;

/// <summary>
///     Represents a session with the application database.
/// </summary>
internal sealed class AppDbContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for creating a new context.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        BlogPosts = Set<BlogPost>();
        Books = Set<Book>();
        CodeSnippets = Set<CodeSnippet>();
        DevChallenges = Set<DevChallenge>();
        LegacyComments = Set<LegacyComment>();
        ProgrammingLanguages = Set<ProgrammingLanguage>();
        Projects = Set<Project>();
        SiteConfiguration = Set<SiteConfiguration>();
        Templates = Set<Template>();
        TutorialArticles = Set<TutorialArticle>();
        TutorialFolders = Set<TutorialFolder>();
        Users = Set<User>();
    }

    /// <summary>
    ///     Gets the collection of blog posts in the database.
    /// </summary>
    /// <value>The collection of blog posts.</value>
    public DbSet<BlogPost> BlogPosts { get; private set; }

    /// <summary>
    ///     Gets the collection of books in the reading list.
    /// </summary>
    /// <value>The collection of books.</value>
    public DbSet<Book> Books { get; private set; }

    /// <summary>
    ///     Gets the collection of code snippets in the database.
    /// </summary>
    /// <value>The collection of code snippets.</value>
    public DbSet<CodeSnippet> CodeSnippets { get; private set; }

    /// <summary>
    ///     Gets the collection of dev challenges in the database.
    /// </summary>
    /// <value>The collection of dev challenges.</value>
    public DbSet<DevChallenge> DevChallenges { get; private set; }

    /// <summary>
    ///     Gets the collection of legacy comments in the database.
    /// </summary>
    /// <value>The collection of legacy comments.</value>
    public DbSet<LegacyComment> LegacyComments { get; private set; }

    /// <summary>
    ///     Gets the collection of programming languages in the database.
    /// </summary>
    /// <value>The collection of programming languages.</value>
    public DbSet<ProgrammingLanguage> ProgrammingLanguages { get; private set; }

    /// <summary>
    ///     Gets the collection of projects in the database.
    /// </summary>
    /// <value>The collection of projects.</value>
    public DbSet<Project> Projects { get; private set; }

    /// <summary>
    ///     Gets the set of site configuration items.
    /// </summary>
    /// <value>The set of site configuration items.</value>
    public DbSet<SiteConfiguration> SiteConfiguration { get; private set; }

    /// <summary>
    ///     Gets the collection of templates in the database.
    /// </summary>
    /// <value>The collection of templates.</value>
    public DbSet<Template> Templates { get; private set; }

    /// <summary>
    ///     Gets the collection of tutorial articles in the database.
    /// </summary>
    /// <value>The collection of tutorial articles.</value>
    public DbSet<TutorialArticle> TutorialArticles { get; private set; }

    /// <summary>
    ///     Gets the collection of tutorial folders in the database.
    /// </summary>
    /// <value>The collection of tutorial folders.</value>
    public DbSet<TutorialFolder> TutorialFolders { get; private set; }

    /// <summary>
    ///     Gets the collection of users in the database.
    /// </summary>
    /// <value>The collection of users.</value>
    public DbSet<User> Users { get; private set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasPostgresEnum<BlogPostType>("public", "blog_post_type", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<BookState>("public", "book_state", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<ProjectStatus>("public", "project_status", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<Visibility>("public", "visibility", new NpgsqlSnakeCaseNameTranslator());

        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new CodeSnippetConfiguration());
        modelBuilder.ApplyConfiguration(new DevChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new LegacyCommentConfiguration());
        modelBuilder.ApplyConfiguration(new ProgrammingLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialArticleConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialFolderConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
