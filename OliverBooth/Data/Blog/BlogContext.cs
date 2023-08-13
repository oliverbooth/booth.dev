using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog.Configuration;

namespace OliverBooth.Data.Blog;

/// <summary>
///     Represents a session with the blog database.
/// </summary>
internal sealed class BlogContext : DbContext
{
    private readonly IConfiguration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogContext" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public BlogContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    ///     Gets the collection of blog posts in the database.
    /// </summary>
    /// <value>The collection of blog posts.</value>
    public DbSet<BlogPost> BlogPosts { get; private set; } = null!;

    /// <summary>
    ///     Gets the collection of users in the database.
    /// </summary>
    /// <value>The collection of users.</value>
    public DbSet<User> Users { get; private set; } = null!;

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = _configuration.GetConnectionString("Blog") ?? string.Empty;
        ServerVersion serverVersion = ServerVersion.AutoDetect(connectionString);
        optionsBuilder.UseMySql(connectionString, serverVersion);
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
