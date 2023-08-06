using Microsoft.EntityFrameworkCore;

namespace OliverBooth.Data;

/// <summary>
///     Represents a session with the web database.
/// </summary>
public sealed class WebContext : DbContext
{
    private readonly IConfiguration _configuration;

    public WebContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString("Web") ?? string.Empty;
        ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, serverVersion);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}
