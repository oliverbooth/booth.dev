using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for the links that belong to projects and creations.
/// </summary>
public sealed class LinkService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LinkService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public LinkService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the links of a project or creation, in order.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <returns>The links.</returns>
    public IReadOnlyList<LinkItem> GetLinks(Guid ownerId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return
        [
            .. dbContext.Links
                .Where(l => l.ProjectId == ownerId || l.CreationId == ownerId)
                .OrderBy(l => l.Position)
                .AsEnumerable()
                .Select(ToItem)
        ];
    }

    /// <summary>
    ///     Adds a link to a project or creation, after its other links.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="kind">The kind of place the link leads to.</param>
    /// <param name="url">The address the link leads to.</param>
    /// <param name="label">The text to show in place of the default for the kind, if any.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the link could not be added.</returns>
    public Result Add(Guid ownerId, LinkKind kind, string url, string? label)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var isProject = dbContext.Projects.Any(p => p.Id == ownerId);
        if (!isProject && !dbContext.Creations.Any(c => c.Id == ownerId))
        {
            return Result.Fail($"No project or creation with ID {ownerId} was found");
        }

        var urlResult = ValidateUrl(url);
        if (urlResult.IsFailed)
        {
            return urlResult.ToResult();
        }

        var position = dbContext.Links
            .Where(l => l.ProjectId == ownerId || l.CreationId == ownerId)
            .Select(l => (int?)l.Position)
            .Max() + 1 ?? 0;

        dbContext.Links.Add(new Link
        {
            ProjectId = isProject ? ownerId : null,
            CreationId = isProject ? null : ownerId,
            Kind = kind,
            Url = urlResult.Value,
            Label = NormalizeLabel(label),
            Position = position
        });
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Changes the kind, address, and label of a link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <param name="kind">The kind of place the link leads to.</param>
    /// <param name="url">The address the link leads to.</param>
    /// <param name="label">The text to show in place of the default for the kind, if any.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the link could not be changed.</returns>
    public Result Update(Guid ownerId, Guid linkId, LinkKind kind, string url, string? label)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var link = FindOwned(dbContext, ownerId, linkId);
        if (link is null)
        {
            return Result.Fail($"The link with ID {linkId} was not found");
        }

        var urlResult = ValidateUrl(url);
        if (urlResult.IsFailed)
        {
            return urlResult.ToResult();
        }

        link.Kind = kind;
        link.Url = urlResult.Value;
        link.Label = NormalizeLabel(label);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Removes a link.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the link could not be removed.</returns>
    public Result Remove(Guid ownerId, Guid linkId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var link = FindOwned(dbContext, ownerId, linkId);
        if (link is null)
        {
            return Result.Fail($"The link with ID {linkId} was not found");
        }

        dbContext.Links.Remove(link);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Makes a link its owner's primary link, replacing the previous one.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="linkId">The ID of the link.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the link could not be made primary.</returns>
    public Result SetPrimary(Guid ownerId, Guid linkId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var link = FindOwned(dbContext, ownerId, linkId);
        if (link is null)
        {
            return Result.Fail($"The link with ID {linkId} was not found");
        }

        using var transaction = dbContext.Database.BeginTransaction();
        ClearPrimaryLinks(dbContext, ownerId);

        link.IsPrimary = true;
        dbContext.SaveChanges();
        transaction.Commit();

        return Result.Ok();
    }

    /// <summary>
    ///     Clears a project or creation's primary link, leaving every link equal.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result ClearPrimary(Guid ownerId)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        ClearPrimaryLinks(dbContext, ownerId);

        return Result.Ok();
    }

    /// <summary>
    ///     Reassigns the position of an owner's links to match their place in <paramref name="orderedIds" />.
    /// </summary>
    /// <param name="ownerId">The ID of the project or creation.</param>
    /// <param name="orderedIds">The IDs of the links, in their new order.</param>
    /// <returns>A <see cref="Result" /> indicating success.</returns>
    public Result Reorder(Guid ownerId, IReadOnlyList<Guid> orderedIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var links = dbContext.Links
            .Where(l => (l.ProjectId == ownerId || l.CreationId == ownerId) && orderedIds.Contains(l.Id))
            .ToDictionary(l => l.Id);

        for (var i = 0; i < orderedIds.Count; i++)
        {
            if (links.TryGetValue(orderedIds[i], out var link))
            {
                link.Position = i;
            }
        }

        dbContext.SaveChanges();
        return Result.Ok();
    }

    private static void ClearPrimaryLinks(AppDbContext dbContext, Guid ownerId)
    {
        dbContext.Links
            .Where(l => (l.ProjectId == ownerId || l.CreationId == ownerId) && l.IsPrimary)
            .ExecuteUpdate(s => s.SetProperty(l => l.IsPrimary, false));
    }

    private static Link? FindOwned(AppDbContext dbContext, Guid ownerId, Guid linkId)
    {
        return dbContext.Links.FirstOrDefault(l => l.Id == linkId && (l.ProjectId == ownerId || l.CreationId == ownerId));
    }

    /// <summary>
    ///     Checks that an address is an absolute <c>http</c> or <c>https</c> URL. Anything else, such as a
    ///     <c>javascript:</c> address, would run in the visitor's browser when the link is clicked.
    /// </summary>
    private static Result<string> ValidateUrl(string url)
    {
        var trimmed = url.Trim();
        var isWebAddress = Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        return isWebAddress
            ? trimmed
            : Result.Fail<string>($"'{trimmed}' isn't a web address starting with http:// or https://.");
    }

    private static string? NormalizeLabel(string? label)
    {
        return string.IsNullOrWhiteSpace(label) ? null : label.Trim();
    }

    private static LinkItem ToItem(Link link)
    {
        return new LinkItem(link.Id, link.Kind, link.Url, link.Label, link.IsPrimary);
    }
}
