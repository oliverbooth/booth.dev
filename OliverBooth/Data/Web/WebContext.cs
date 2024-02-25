using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web.Configuration;

namespace OliverBooth.Data.Web;

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
    ///     Gets the collection of blacklist entries in the database.
    /// </summary>
    /// <value>The collection of blacklist entries.</value>
    public DbSet<BlacklistEntry> ContactBlacklist { get; private set; } = null!;

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
    ///     Gets the collection of sessions in the database.
    /// </summary>
    /// <value>The collection of sessions.</value>
    public DbSet<Session> Sessions { get; private set; } = null!;

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
    ///     Gets the collection of users in the database.
    /// </summary>
    /// <value>The collection of users.</value>
    public DbSet<User> Users { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString("Web") ?? string.Empty;
        ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, serverVersion);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BlacklistEntryConfiguration());
        modelBuilder.ApplyConfiguration(new BookConfiguration());
        modelBuilder.ApplyConfiguration(new ProgrammingLanguageConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfigurationConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
