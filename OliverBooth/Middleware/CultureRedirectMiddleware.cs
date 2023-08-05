using System.Globalization;

namespace OliverBooth.Middleware;

/// <summary>
///     Redirects requests to the default culture if the culture is not specified in the URL.
/// </summary>
internal sealed class CultureRedirectMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CultureRedirectMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next request delegate.</param>
    public CultureRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///     Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task Invoke(HttpContext context)
    {
        const StringComparison comparison = StringComparison.OrdinalIgnoreCase;

        HttpRequest request = context.Request;
        PathString requestPath = request.Path;

        if (requestPath.HasValue)
        {
            string[] pathSegments = requestPath.Value.Split('/');
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            if (pathSegments.Length >= 2 && cultures.Any(CultureMatch))
            {
                await _next(context);
                return;
            }

            bool CultureMatch(CultureInfo culture)
            {
                string segment = pathSegments[1].Split('-')[0];
                return string.Equals(culture.TwoLetterISOLanguageName, segment, comparison);
            }
        }

        const string defaultCulture = "en";
        var redirectUrl = $"/{defaultCulture}{requestPath}{request.QueryString}";
        context.Response.Redirect(redirectUrl);
    }
}