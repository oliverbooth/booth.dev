using System.ComponentModel;
using System.Diagnostics;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Cysharp.Text;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using X10D.Text;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing tutorials.
/// </summary>
public sealed class TutorialService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TutorialService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    public TutorialService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets the total number of articles, optionally filtered by visibility. Trashed articles are excluded.
    /// </summary>
    /// <param name="visibility">
    ///     The visibility to filter by. If set to <see cref="Visibility.None" />, counts all non-trashed articles
    ///     regardless of visibility.
    /// </param>
    /// <returns>The total number of articles.</returns>
    public int GetArticleCount(Visibility visibility = Visibility.None)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return visibility switch
        {
            Visibility.None => context.TutorialArticles.Count(a => a.TrashedAt == null),
            _ => context.TutorialArticles.Count(a => a.TrashedAt == null && a.CurrentDraft!.Visibility == visibility)
        };
    }

    /// <summary>
    ///     Gets the articles within a tutorial folder. Trashed articles are excluded.
    /// </summary>
    /// <param name="folder">The folder whose articles to retrieve.</param>
    /// <param name="visibility">The visibility to filter by. -1 does not filter.</param>
    /// <returns>A read-only view of the articles in the folder.</returns>
    public IReadOnlyList<TutorialArticle> GetArticles(TutorialFolder folder,
        Visibility visibility = Visibility.None)
    {
        if (folder is null)
        {
            throw new ArgumentNullException(nameof(folder));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IQueryable<TutorialArticle> articles = context.TutorialArticles.Include(a => a.CurrentDraft)
            .Where(a => a.TrashedAt == null && a.CurrentDraft!.Folder == folder.Id);

        if (visibility != Visibility.None)
        {
            articles = articles.Where(a => a.CurrentDraft!.Visibility == visibility);
        }

        return [.. articles.OrderBy(a => a.CurrentDraft!.Rank)];
    }

    /// <summary>
    ///     Gets the folders within a tutorial folder.
    /// </summary>
    /// <param name="parent">The parent folder whose child folders to retrieve.</param>
    /// <param name="visibility">The visibility to filter by. -1 does not filter.</param>
    /// <returns>A read-only view of the folders in the parent folder.</returns>
    public IReadOnlyList<TutorialFolder> GetFolders(TutorialFolder? parent = null,
        Visibility visibility = Visibility.None)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IQueryable<TutorialFolder> folders = context.TutorialFolders;

        folders = parent is null ? folders.Where(f => f.Parent == null) : folders.Where(f => f.Parent == parent.Id);
        if (visibility != Visibility.None)
        {
            folders = folders.Where(a => a.Visibility == visibility);
        }

        return [.. folders.OrderBy(f => f.Rank)];
    }

    /// <summary>
    ///     Retrieves a folder by its ID.
    /// </summary>
    /// <param name="id">The ID of the folder to retrieve.</param>
    /// <returns>A <see cref="Result{T}" /> containing the folder if that folder was found, or a failure if not found.</returns>
    public Result<TutorialFolder> GetFolder(Guid id)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var folder = context.TutorialFolders.FirstOrDefault(f => f.Id == id);
        return folder is not null ? folder : Result.Fail("Folder not found");
    }

    /// <summary>
    ///     Gets the full slug of the specified folder.
    /// </summary>
    /// <param name="folder">The folder whose slug to return.</param>
    /// <returns>The full slug of the folder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="folder" /> is <see langword="null" />.</exception>
    public string GetFullSlug(TutorialFolder folder)
    {
        if (folder is null)
        {
            throw new ArgumentNullException(nameof(folder));
        }

        var folderStack = new Stack<TutorialFolder>();
        folderStack.Push(folder);

        while (folder.Parent is { } parentId)
        {
            Result<TutorialFolder> currentResult = GetFolder(parentId);
            if (currentResult.IsFailed)
            {
                break;
            }

            folderStack.Push(currentResult.Value);
        }

        using var builder = ZString.CreateUtf8StringBuilder();

        while (folderStack.Count > 0)
        {
            builder.Append(folderStack.Pop().Slug);

            if (folderStack.Count > 0)
            {
                builder.Append('/');
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Gets the full slug of the specified article.
    /// </summary>
    /// <param name="article">The article whose slug to return.</param>
    /// <returns>The full slug of the article.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="article" /> is <see langword="null" />.</exception>
    public string GetFullSlug(TutorialArticle article)
    {
        if (article is null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        Result<TutorialFolder> folderResult = GetFolder(article.Folder);
        if (folderResult.IsFailed)
        {
            return article.Slug;
        }

        return $"{GetFullSlug(folderResult.Value)}/{article.Slug}";
    }

    /// <summary>
    ///     Returns the most recent tutorial articles, limited to the specified count. Trashed articles are excluded.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving tutorial articles.</param>
    /// <returns>A read-only list of the most recent tutorial articles.</returns>
    public IReadOnlyList<TutorialArticle> GetRecentArticles(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var articles = context.TutorialArticles.Include(a => a.CurrentDraft).Where(a => a.TrashedAt == null);

        if (searchOptions.Visibility != Visibility.None)
        {
            articles = articles.Where(p => p.CurrentDraft!.Visibility == searchOptions.Visibility);
        }

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => articles.OrderByDescending(p => p.Published),
            ActivitySortStrategy.Updated => articles.OrderByDescending(p => p.Updated ?? p.Published),
            _ => throw new InvalidEnumArgumentException(nameof(searchOptions.SortStrategy),
                (int)searchOptions.SortStrategy,
                typeof(ActivitySortStrategy))
        };

        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Retrieves an article by its ID.
    /// </summary>
    /// <param name="id">The ID of the article.</param>
    /// <param name="includeTrashed">
    ///     Whether to include the article if it's trashed. Only the admin editor should pass <see langword="true" />
    ///     — every public-facing caller should get the trash exclusion for free.
    /// </param>
    /// <returns>A <see cref="Result{T}" /> containing the article if that article was found, or a failure if not found.</returns>
    public Result<TutorialArticle> GetArticle(Guid id, bool includeTrashed = false)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Include(a => a.CurrentDraft).FirstOrDefault(a => a.Id == id);

        if (article is null || (article.TrashedAt is not null && !includeTrashed))
        {
            return Result.Fail("Article not found");
        }

        return article;
    }

    /// <summary>
    ///     Retrieves an article by its slug, optionally within a specified parent folder. Trashed articles are
    ///     excluded.
    /// </summary>
    /// <param name="slug">The slug of the article, which may include folder slugs separated by '/'.</param>
    /// <param name="parentFolder">The parent folder within which to search for the article.</param>
    /// <returns>A <see cref="Result{T}" /> containing the article if that article was found, or a failure if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    public Result<TutorialArticle> GetArticle(string slug, TutorialFolder? parentFolder = null)
    {
        if (slug is null)
        {
            throw new ArgumentNullException(nameof(slug));
        }

        var tokens = slug.Split('/');
        TutorialFolder? folder = parentFolder;

        for (var index = 0; index < tokens.Length - 1; index++)
        {
            var folderResult = GetFolder(tokens[index], folder);
            if (folderResult.IsFailed)
            {
                return Result.Fail("Folder not found");
            }

            folder = folderResult.Value;
        }

        if (folder is null)
        {
            return Result.Fail("Folder not found");
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        slug = tokens[^1];
        var article = context.TutorialArticles.Include(a => a.CurrentDraft)
            .FirstOrDefault(a => a.Slug == slug && a.TrashedAt == null && a.CurrentDraft!.Folder == folder.Id);
        return article is not null ? article : Result.Fail("Article not found");
    }

    /// <summary>
    ///     Retrieves a folder by its full slug, which may include parent folder slugs separated by '/'.
    /// </summary>
    /// <param name="slug">The full slug of the folder, which may include parent folder slugs separated by '/'.</param>
    /// <returns>A <see cref="Result{T}" /> containing the folder if that folder was found, or a failure if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    public Result<TutorialFolder> GetFolder(ReadOnlySpan<char> slug)
    {
        TutorialFolder? currentFolder = null;

        var folderCount = slug.CountSubstring('/') + 1;
        Span<Range> ranges = stackalloc Range[folderCount];
        var rangeCount = slug.SplitAny(ranges, "/");

        if (folderCount != rangeCount)
        {
            throw new UnreachableException($"Split count mismatch: expected {folderCount} tokens, got {rangeCount}.");
        }

        for (var index = 0; index < rangeCount; index++)
        {
            ReadOnlySpan<char> token = slug[ranges[index]];
            var folderResult = GetFolder(token, currentFolder);
            if (folderResult.IsFailed)
            {
                return Result.Fail("Folder not found");
            }

            currentFolder = folderResult.Value;
        }

        return currentFolder is not null ? currentFolder : Result.Fail("Folder not found");
    }

    /// <summary>
    ///     Retrieves a folder by its slug.
    /// </summary>
    /// <param name="slug">The slug of the folder.</param>
    /// <param name="parent">The parent folder.</param>
    /// <returns>A <see cref="Result{T}" /> containing the folder if that folder was found, or a failure if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    public Result<TutorialFolder> GetFolder(ReadOnlySpan<char> slug, TutorialFolder? parent)
    {
        var slugString = slug.ToString();

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var folder = parent is null
            ? context.TutorialFolders.FirstOrDefault(a => a.Slug == slugString)
            : context.TutorialFolders.FirstOrDefault(a => a.Slug == slugString && a.Parent == parent.Id);
        return folder is not null ? folder : Result.Fail("Folder not found");
    }
}
