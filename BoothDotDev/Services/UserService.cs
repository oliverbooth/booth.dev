using System.Collections.Concurrent;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing users.
/// </summary>
public sealed class UserService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, User> _userCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="AppDbContext" />.
    /// </param>
    public UserService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Finds a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to find.</param>
    /// <returns>A <see cref="Result{T}" /> containing the user if found; otherwise, an error result.</returns>
    public Result<User> GetUser(Guid id)
    {
        if (_userCache.TryGetValue(id, out var user))
        {
            return user;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null)
        {
            _userCache.TryAdd(id, user);
        }

        return user is not null ? Result.Ok(user) : Result.Fail("User not found.");
    }
}
