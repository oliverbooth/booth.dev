using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service for retrieving configuration values.
/// </summary>
public sealed class ConfigurationService
{
    private readonly IDbContextFactory<WebContext> _webContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationService" /> class.
    /// </summary>
    /// <param name="webContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> for the <see cref="WebContext" />.
    /// </param>
    public ConfigurationService(IDbContextFactory<WebContext> webContextFactory)
    {
        _webContextFactory = webContextFactory;
    }

    /// <summary>
    ///     Gets the value of a site configuration key.
    /// </summary>
    /// <param name="key">The key of the site configuration item.</param>
    /// <returns>The value of the site configuration item.</returns>
    public string? GetSiteConfiguration(string key)
    {
        using WebContext context = _webContextFactory.CreateDbContext();
        return context.SiteConfiguration.Find(key)?.Value;
    }
}
