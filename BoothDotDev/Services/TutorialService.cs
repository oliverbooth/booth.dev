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
    private const string Area = "tutorial";

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly CdnMediaService _cdnMediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TutorialService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    /// <param name="cdnMediaService">The <see cref="CdnMediaService" />.</param>
    public TutorialService(IDbContextFactory<AppDbContext> dbContextFactory, CdnMediaService cdnMediaService)
    {
        _dbContextFactory = dbContextFactory;
        _cdnMediaService = cdnMediaService;
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
    ///     Gets every tutorial folder, regardless of nesting depth, in title order.
    /// </summary>
    /// <returns>A read-only view of every folder.</returns>
    public IReadOnlyList<TutorialFolder> GetAllFolders()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.TutorialFolders.OrderBy(f => f.Title)];
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
    ///     Creates a new tutorial folder.
    /// </summary>
    /// <param name="request">The folder's fields.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created folder.</returns>
    public Result<TutorialFolder> CreateFolder(TutorialFolderSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var folder = new TutorialFolder();
        ApplyFolderRequest(folder, request);

        context.TutorialFolders.Add(folder);
        context.SaveChanges();

        return folder;
    }

    /// <summary>
    ///     Updates an existing tutorial folder.
    /// </summary>
    /// <param name="id">The ID of the folder to update.</param>
    /// <param name="request">The folder's new fields.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated folder, or an error if no folder with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<TutorialFolder> UpdateFolder(Guid id, TutorialFolderSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var folder = context.TutorialFolders.Find(id);

        if (folder is null)
        {
            return Result.Fail($"The folder with ID {id} was not found");
        }

        ApplyFolderRequest(folder, request);
        context.SaveChanges();

        return folder;
    }

    /// <summary>
    ///     Deletes a tutorial folder. The folder must not contain any child folders or articles.
    /// </summary>
    /// <param name="id">The ID of the folder to delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if the folder was not found or is not empty.
    /// </returns>
    public Result DeleteFolder(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var folder = context.TutorialFolders.Find(id);

        if (folder is null)
        {
            return Result.Fail($"The folder with ID {id} was not found");
        }

        var hasChildFolders = context.TutorialFolders.Any(f => f.Parent == id);
        if (hasChildFolders)
        {
            return Result.Fail("This folder contains subfolders. Move or delete them first.");
        }

        var hasArticles = context.TutorialArticles.Any(a => a.TrashedAt == null && a.CurrentDraft!.Folder == id);
        if (hasArticles)
        {
            return Result.Fail("This folder contains articles. Move or trash them first.");
        }

        context.TutorialFolders.Remove(folder);
        context.SaveChanges();

        return Result.Ok();
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

            folder = currentResult.Value;
            folderStack.Push(folder);
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
            ActivitySortStrategy.Published => articles.OrderByDescending(p => p.PublishedAt),
            ActivitySortStrategy.Updated => articles.OrderByDescending(p => p.UpdatedAt ?? p.PublishedAt),
            _ => throw new InvalidEnumArgumentException(nameof(searchOptions.SortStrategy),
                (int)searchOptions.SortStrategy,
                typeof(ActivitySortStrategy))
        };

        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Gets every article, newest first, excluding trashed ones.
    /// </summary>
    /// <returns>A read-only view of every article.</returns>
    public IReadOnlyList<TutorialArticle> GetAllArticles()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.TutorialArticles.Include(a => a.CurrentDraft).Where(a => a.TrashedAt == null)
                .OrderByDescending(a => a.PublishedAt)
        ];
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

    /// <summary>
    ///     Creates a new article, along with its first draft, which immediately becomes the article's current draft.
    /// </summary>
    /// <param name="request">The article's parent-level fields and the content of its first draft.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created article.</returns>
    public Result<TutorialArticle> CreateArticle(TutorialArticleSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();

        var article = new TutorialArticle
        {
            Slug = request.Slug,
            PublishedAt = request.PublishedAt.ToUniversalTime(),
            EnableComments = request.EnableComments,
            NextPart = request.NextPart,
            PreviousPart = request.PreviousPart,
            RedirectFrom = request.RedirectFrom
        };

        // two SaveChanges calls, not one: TutorialArticle -> TutorialArticleDraft (via TutorialArticleId) and
        // TutorialArticleDraft -> TutorialArticle (via CurrentDraftId) form a cycle between two rows that are
        // both new, which EF can't resolve in a single call even though CurrentDraftId is nullable.
        context.TutorialArticles.Add(article);
        context.SaveChanges();

        var draft = NewDraft(article.Id, request.Content);
        context.TutorialArticleDrafts.Add(draft);
        article.CurrentDraftId = draft.Id;
        context.SaveChanges();

        return article;
    }

    /// <summary>
    ///     Saves a new draft of an existing article, without publishing it.
    /// </summary>
    /// <param name="id">The ID of the article to save a draft for.</param>
    /// <param name="request">The article's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the article the draft was saved for, or an error if no article with
    ///     the specified <paramref name="id" /> exists.
    /// </returns>
    public Result<TutorialArticle> SaveArticleDraft(Guid id, TutorialArticleSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Find(id);

        if (article is null)
        {
            return Result.Fail($"Article with ID '{id}' not found.");
        }

        var draft = NewDraft(article.Id, request.Content);
        context.TutorialArticleDrafts.Add(draft);

        ApplyParentFields(article, request);

        context.SaveChanges();
        return article;
    }

    /// <summary>
    ///     Saves a new draft of an existing article and publishes it, making it the article's current draft.
    /// </summary>
    /// <param name="id">The ID of the article to publish.</param>
    /// <param name="request">The article's parent-level fields and the content of the new draft.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated article, or an error if no article with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<TutorialArticle> PublishArticle(Guid id, TutorialArticleSaveRequest request)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Find(id);

        if (article is null)
        {
            return Result.Fail($"Article with ID '{id}' not found.");
        }

        var draft = NewDraft(article.Id, request.Content);
        context.TutorialArticleDrafts.Add(draft);

        ApplyParentFields(article, request);
        article.CurrentDraftId = draft.Id;
        article.UpdatedAt = DateTimeOffset.UtcNow;

        context.SaveChanges();
        return article;
    }

    /// <summary>
    ///     Moves an article to the trash. It's excluded from every listing and 404s on its public URL, but nothing
    ///     about it is otherwise touched, and it can be restored with <see cref="RestoreArticle" />.
    /// </summary>
    /// <param name="id">The ID of the article to trash.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the trashed article, or an error if no article with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<TutorialArticle> TrashArticle(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Find(id);

        if (article is null)
        {
            return Result.Fail($"The article with ID {id} was not found");
        }

        article.TrashedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        return article;
    }

    /// <summary>
    ///     Restores a trashed article, making it visible in listings and on its public URL again.
    /// </summary>
    /// <param name="id">The ID of the article to restore.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the restored article, or an error if no article with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<TutorialArticle> RestoreArticle(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Find(id);

        if (article is null)
        {
            return Result.Fail($"The article with ID {id} was not found");
        }

        article.TrashedAt = null;
        context.SaveChanges();
        return article;
    }

    /// <summary>
    ///     Permanently deletes a trashed article - the article row, every draft in its revision history (cascade), and every file
    ///     it had uploaded to the CDN. This cannot be undone.
    /// </summary>
    /// <param name="id">The ID of the article to permanently delete.</param>
    /// <returns>
    ///     A <see cref="Result" /> indicating success, or a failure if no article with the specified <paramref name="id" />
    ///     exists or it isn't currently trashed.
    /// </returns>
    public Result PermanentlyDeleteArticle(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var article = context.TutorialArticles.Find(id);

        if (article is null)
        {
            return Result.Fail($"The article with ID {id} was not found");
        }

        if (article.TrashedAt is null)
        {
            return Result.Fail("Only trashed articles can be permanently deleted.");
        }

        _cdnMediaService.DeleteAllMedia(id, article.PublishedAt, Area);

        context.TutorialArticles.Remove(article);
        context.SaveChanges();
        return Result.Ok();
    }

    /// <summary>
    ///     Gets all trashed articles, newest-trashed first.
    /// </summary>
    /// <returns>A read-only view of all trashed articles.</returns>
    public IReadOnlyList<TutorialArticle> GetTrashedArticles()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.TutorialArticles.Include(a => a.CurrentDraft).Where(a => a.TrashedAt != null)
                .OrderByDescending(a => a.TrashedAt)
        ];
    }

    /// <summary>
    ///     Returns an article's full draft history, newest first.
    /// </summary>
    /// <param name="id">The ID of the article whose draft history to return.</param>
    /// <returns>The article's drafts, newest first.</returns>
    public IReadOnlyList<TutorialArticleDraft> GetDraftHistory(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.TutorialArticleDrafts.Where(d => d.TutorialArticleId == id).OrderByDescending(d => d.CreatedAt)
        ];
    }

    /// <summary>
    ///     Returns a specific draft of the specified article, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the article the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't
    ///     belong to the specified article.
    /// </returns>
    public Result<TutorialArticleDraft> GetDraft(Guid id, Guid draftId)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.TutorialArticleDrafts.Find(draftId);

        if (draft is null || draft.TutorialArticleId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for article '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified article, which may or may not be the article's current
    ///     (published) draft.
    /// </summary>
    /// <param name="id">The ID of the article whose newest draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the article's newest draft, or an error if the article has no drafts.
    /// </returns>
    public Result<TutorialArticleDraft> GetNewestDraft(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        var draft = context.TutorialArticleDrafts.Where(d => d.TutorialArticleId == id)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Article '{id}' has no drafts.");
        }

        return draft;
    }

    /// <summary>
    ///     Applies the folder-level fields of a save request onto a folder.
    /// </summary>
    /// <param name="folder">The folder to apply the fields to.</param>
    /// <param name="request">The save request containing the fields to apply.</param>
    private static void ApplyFolderRequest(TutorialFolder folder, TutorialFolderSaveRequest request)
    {
        folder.Title = request.Title;
        folder.Slug = request.Slug;
        folder.Description = request.Description;
        folder.PreviewImageUrl = request.PreviewImageUrl;
        folder.Visibility = request.Visibility;
        folder.Rank = request.Rank;
        folder.Parent = request.Parent;
    }

    /// <summary>
    ///     Applies the parent-level fields of a save request onto an article.
    /// </summary>
    /// <param name="article">The article to apply the fields to.</param>
    /// <param name="request">The save request containing the fields to apply.</param>
    private static void ApplyParentFields(TutorialArticle article, TutorialArticleSaveRequest request)
    {
        article.Slug = request.Slug;
        article.PublishedAt = request.PublishedAt.ToUniversalTime();
        article.EnableComments = request.EnableComments;
        article.NextPart = request.NextPart;
        article.PreviousPart = request.PreviousPart;
        article.RedirectFrom = request.RedirectFrom;
    }

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified article.
    /// </summary>
    /// <param name="tutorialArticleId">The ID of the article for which to create a draft.</param>
    /// <param name="content">The content for the new draft.</param>
    private static TutorialArticleDraft NewDraft(Guid tutorialArticleId, TutorialArticleDraftContent content)
    {
        return new TutorialArticleDraft
        {
            TutorialArticleId = tutorialArticleId,
            Title = content.Title,
            Body = content.Body,
            Excerpt = content.Excerpt,
            Folder = content.Folder,
            Rank = content.Rank,
            PreviewImageUrl = content.PreviewImageUrl,
            ShowTableOfContents = content.ShowTableOfContents,
            TableOfContentsExpanded = content.TableOfContentsExpanded,
            Visibility = content.Visibility
        };
    }
}
