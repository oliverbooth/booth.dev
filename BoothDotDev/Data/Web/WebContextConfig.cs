using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Web;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

namespace BoothDotDev.Data.Web;

/// <summary>
///     Provides configuration for <see cref="WebContext"/>.
/// </summary>
public static class WebContextConfig
{
    /// <summary>
    ///     Configures the web database context.
    /// </summary>
    /// <param name="builder">The options builder for the context.</param>
    /// <param name="connectionString">The connection string for the database.</param>
    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        builder.UseNpgsql(connectionString, options =>
        {
            options.MapEnum<BookState>("book_state", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<Visibility>("visibility", "public", new NpgsqlSnakeCaseNameTranslator());
            options.MapEnum<ProjectStatus>("project_status", "public", new NpgsqlSnakeCaseNameTranslator());
        });
        builder.UseSnakeCaseNamingConvention();
    }
}
