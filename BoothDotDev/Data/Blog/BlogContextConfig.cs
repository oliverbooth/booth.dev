using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Blog;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data.Blog;

/// <summary>
///     Provides configuration for <see cref="BlogContext"/>.
/// </summary>
public static class BlogContextConfig
{
    /// <summary>
    ///     Configures the Blog database context.
    /// </summary>
    /// <param name="builder">The options builder for the context.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseNpgsql(connectionString, options =>
        {
            options.MapEnum<Visibility>("visibility", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<BlogPostType>("blog_post_type", "public", new NpgsqlSnakeCaseNameTranslator());
        });
        builder.UseSnakeCaseNamingConvention();
    }
}
