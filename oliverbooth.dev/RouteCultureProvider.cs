using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Localization;

namespace OliverBooth;

internal sealed partial class RouteCultureProvider : IRequestCultureProvider
{
    private static readonly Regex CultureRegex = GetCultureRegex();
    private readonly CultureInfo _defaultCulture;
    private readonly CultureInfo _defaultUiCulture;

    public RouteCultureProvider(CultureInfo requestCulture) : this(new RequestCulture(requestCulture))
    {
    }

    public RouteCultureProvider(RequestCulture requestCulture)
    {
        _defaultCulture = requestCulture.Culture;
        _defaultUiCulture = requestCulture.UICulture;
    }

    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        PathString url = httpContext.Request.Path;

        string defaultCulture = _defaultCulture.TwoLetterISOLanguageName;
        string defaultUiCulture = _defaultUiCulture.TwoLetterISOLanguageName;

        if (url.ToString().Length <= 1)
        {
            return Task.FromResult(new ProviderCultureResult(defaultCulture, defaultUiCulture))!;
        }

        string[]? parts = httpContext.Request.Path.Value?.Split('/');
        string requestCulture = parts?[1] ?? string.Empty;

        bool isMatch = CultureRegex.IsMatch(requestCulture);
        string culture = isMatch ? requestCulture : defaultCulture;
        string uiCulture = isMatch ? requestCulture : defaultUiCulture;

        return Task.FromResult(new ProviderCultureResult(culture, uiCulture))!;
    }

    [GeneratedRegex("^[a-z]{2}(-[A-Z]{2})*$")]
    private static partial Regex GetCultureRegex();
}
