using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Blog.Data;

namespace OliverBooth.Blog.Services;

/// <summary>
///     Represents an implementation of <see cref="IUserService" />.
/// </summary>
internal sealed class UserService : IUserService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    public UserService(IDbContextFactory<BlogContext> dbContextFactory)
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
