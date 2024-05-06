using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <inheritdoc cref="IContactService" />
internal sealed class ContactService : IContactService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContactService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    public ContactService(IDbContextFactory<WebContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<IBlacklistEntry> GetBlacklist()
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return context.ContactBlacklist.OrderBy(b => b.EmailAddress).ToArray();
    }
}
