using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data;

/// <summary>
///     Provides methods for configuring the <see cref="AppDbContext" /> database context.
/// </summary>
public static class AppDbContextConfig
{
    /// <summary>
    ///     Configures the <see cref="AppDbContext" /> database context with the specified connection string.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder" /> to configure.</param>
    /// <param name="connectionString">The connection string to use.</param>
    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseNpgsql(connectionString, options =>
        {
            options.MapEnum<BookState>("book_state", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<FontStyle>("font_style", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<ProjectStatus>("project_status", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<ProjectType>("project_type", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<Visibility>("visibility", "public", new NpgsqlSnakeCaseNameTranslator());
        });
        builder.UseSnakeCaseNamingConvention();
    }
}
