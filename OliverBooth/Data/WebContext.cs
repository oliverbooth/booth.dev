using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;
using OliverBooth.Data.Web.Configuration;

namespace OliverBooth.Data;

/// <summary>
///     Represents a session with the web database.
/// </summary>
public sealed class WebContext : DbContext
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
    ///     Gets the set of site configuration items.
    /// </summary>
    /// <value>The set of site configuration items.</value>
    public DbSet<SiteConfiguration> SiteConfiguration { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of templates in the database.
    /// </summary>
    /// <value>The collection of templates.</value>
    public DbSet<Template> Templates { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString("Web") ?? string.Empty;
        ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, serverVersion);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TemplateConfiguration());
        modelBuilder.ApplyConfiguration(new SiteConfigurationConfiguration());
    }
}
