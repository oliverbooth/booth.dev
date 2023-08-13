using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogUserService" />.
/// </summary>
internal sealed class BlogUserService : IBlogUserService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogUserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    public BlogUserService(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <inheritdoc />
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);
        return user is not null;
    }
}
