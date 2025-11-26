using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using BoothDotDev.Data.Blog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogUserService" />.
/// </summary>
internal sealed class BlogUserService : IBlogUserService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly ConcurrentDictionary<Guid, IUser> _userCache = new();

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
    public async Task SignInAsync(HttpContext httpContext, IUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Email, user.EmailAddress)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    /// <inheritdoc />
    public bool TryGetUser(Guid id, [NotNullWhen(true)] out IUser? user)
    {
        if (_userCache.TryGetValue(id, out user)) return true;

        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.Find(id);

        if (user is not null) _userCache.TryAdd(id, user);
        return user is not null;
    }

    /// <inheritdoc />
    public bool TryGetUser(string email, [NotNullWhen(true)] out IUser? user)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        user = context.Users.FirstOrDefault(u => u.EmailAddress == email);
        if (user is not null) _userCache.TryAdd(user.Id, user);
        return user is not null;
    }
}
