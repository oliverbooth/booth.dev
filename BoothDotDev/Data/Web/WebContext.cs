using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;
using BoothDotDev.Data.Web.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data.Web;

/// <summary>
///     Represents a session with the web database.
/// </summary>
internal sealed class WebContext : DbContext
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WebContext" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public WebContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets the collection of books in the reading list.
    /// </summary>
    /// <value>The collection of books.</value>
    public DbSet<Book> Books { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of code snippets in the database.
    /// </summary>
    /// <value>The collection of code snippets.</value>
    public DbSet<CodeSnippet> CodeSnippets { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of dev challenges in the database.
    /// </summary>
    /// <value>The collection of dev challenges.</value>
    public DbSet<DevChallenge> DevChallenges { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of programming languages in the database.
    /// </summary>
    /// <value>The collection of programming languages.</value>
    public DbSet<ProgrammingLanguage> ProgrammingLanguages { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of projects in the database.
    /// </summary>
    /// <value>The collection of projects.</value>
    public DbSet<Project> Projects { get; private set; } = null!;

    /// <summary>
    ///     Gets the set of site configuration items.
    /// </summary>
    /// <value>The set of site configuration items.</value>
    public DbSet<SiteConfiguration> SiteConfiguration { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of templates in the database.
    /// </summary>
    /// <value>The collection of templates.</value>
    public DbSet<Template> Templates { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of tutorial articles in the database.
    /// </summary>
    /// <value>The collection of tutorial articles.</value>
    public DbSet<TutorialArticle> TutorialArticles { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of tutorial folders in the database.
    /// </summary>
    /// <value>The collection of tutorial folders.</value>
    public DbSet<TutorialFolder> TutorialFolders { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString("Web") ?? string.Empty;
        optionsBuilder.UseNpgsql(connectionString, o =>
        {
            o.MapEnum<BookState>("book_state");
            o.MapEnum<ProjectStatus>("project_status");
            o.MapEnum<Visibility>("visibility");
        });
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.HasPostgresEnum<BookState>("public", "book_state", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<ProjectStatus>("public", "project_status", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<Visibility>("public", "visibility", new NpgsqlSnakeCaseNameTranslator());

        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new CodeSnippetConfiguration());
        modelBuilder.ApplyConfiguration(new DevChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new ProgrammingLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialArticleConfiguration());
        modelBuilder.ApplyConfiguration(new TutorialFolderConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfigurationConfiguration());
    }
}
