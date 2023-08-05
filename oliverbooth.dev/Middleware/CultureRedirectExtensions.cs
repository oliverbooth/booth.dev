namespace OliverBooth.Middleware;

/// <summary>
///     Extension methods for <see cref="CultureRedirectMiddleware" />.
/// </summary>
internal static class CultureRedirectExtensions
{
    /// <summary>
    ///     Adds the <see cref="CultureRedirectMiddleware" /> to the application's request pipeline.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseCultureRedirect(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CultureRedirectMiddleware>();
    }
}
