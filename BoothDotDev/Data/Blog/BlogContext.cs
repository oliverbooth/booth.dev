using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Data.Blog.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data.Blog;

/// <summary>
///     Represents a session with the blog database.
/// </summary>
internal sealed class BlogContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogContext" /> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
        BlogPosts = Set<BlogPost>();
        LegacyComments = Set<LegacyComment>();
        Users = Set<User>();
    }

    /// <summary>
    ///     Gets the collection of blog posts in the database.
    /// </summary>
    /// <value>The collection of blog posts.</value>
    public DbSet<BlogPost> BlogPosts { get; private set; }

    /// <summary>
    ///     Gets the collection of legacy comments in the database.
    /// </summary>
    /// <value>The collection of legacy comments.</value>
    public DbSet<LegacyComment> LegacyComments { get; private set; }

    /// <summary>
    ///     Gets the collection of users in the database.
    /// </summary>
    /// <value>The collection of users.</value>
    public DbSet<User> Users { get; private set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("blog");
        modelBuilder.HasPostgresEnum<BlogPostType>("public", "blog_post_type", new NpgsqlSnakeCaseNameTranslator());
        modelBuilder.HasPostgresEnum<Visibility>("public", "visibility", new NpgsqlSnakeCaseNameTranslator());

        modelBuilder.ApplyConfiguration(new BlogPostConfiguration());
        modelBuilder.ApplyConfiguration(new LegacyCommentConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
    }
}
