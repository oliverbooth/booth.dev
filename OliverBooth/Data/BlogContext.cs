using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Blog.Configuration;

namespace OliverBooth.Data;

/// <summary>
///     Represents a session with the blog database.
/// </summary>
public sealed class BlogContext : DbContext
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
    ///     Gets the set of authors.
    /// </summary>
    /// <value>The set of authors.</value>
    public DbSet<Author> Authors { get; internal set; } = null!;

    /// <summary>
    ///     Gets the set of blog posts.
    /// </summary>
    /// <value>The set of blog posts.</value>
    public DbSet<BlogPost> BlogPosts { get; internal set; } = null!;

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
        modelBuilder.ApplyConfiguration(new AuthorConfiguration());
        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
    }
}
