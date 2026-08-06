using BoothDotDev.Common.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data;

/// <summary>
///     Provides configuration for <see cref="AppDbContext"/>.
/// </summary>
public static class AppDbContextConfig
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
            options.MapEnum<BlogPostType>("blog_post_type", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<BookState>("book_state", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<ProjectStatus>("project_status", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<Visibility>("visibility", "public", new NpgsqlSnakeCaseNameTranslator());
        });
        builder.UseSnakeCaseNamingConvention();
    }
}
