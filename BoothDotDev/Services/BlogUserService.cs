using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public sealed class BlogUserService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, User> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogUserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="AppDbContext" />.
    /// </param>
    public BlogUserService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Attempts to find a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to find.</param>
    /// <param name="user">
    ///     When this method returns, contains the user with the specified ID, if the user is found; otherwise,
    ///     <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a user with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out User? user)
    {
        if (_userCache.TryGetValue(id, out user))
        {
            return true;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null)
        {
            _userCache.TryAdd(id, user);
        }

        return user is not null;
    }
}
