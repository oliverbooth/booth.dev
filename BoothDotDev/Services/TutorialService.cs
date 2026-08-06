using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;
using Cysharp.Text;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for managing tutorials.
/// </summary>
public sealed class TutorialService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TutorialService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public TutorialService(IDbContextFactory<AppDbContext> dbContextFactory,
        MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
    }

    /// <summary>
    ///     Gets the articles within a tutorial folder.
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
        IQueryable<TutorialArticle> articles = context.TutorialArticles.Where(a => a.Folder == folder.Id);

        if (visibility != Visibility.None)
        {
            articles = articles.Where(a => a.Visibility == visibility);
        }

        return [.. articles.OrderBy(a => a.Rank)];
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
    ///     Gets a folder by its ID.
    /// </summary>
    /// <param name="id">The ID of the folder to get.</param>
    /// <returns>The folder, or <see langword="null" /> if not found.</returns>
    public TutorialFolder? GetFolder(Guid id)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.TutorialFolders.FirstOrDefault(f => f.Id == id);
    }

    /// <summary>
    ///     Gets a folder by its slug.
    /// </summary>
    /// <param name="slug">The slug of the folder.</param>
    /// <param name="parent">The parent folder.</param>
    /// <returns>The folder, or <see langword="null" /> if not found.</returns>
    [return: NotNullIfNotNull(nameof(slug))]
    public TutorialFolder? GetFolder(string? slug, TutorialFolder? parent = null)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return parent is null
            ? context.TutorialFolders.FirstOrDefault(a => a.Slug == slug)
            : context.TutorialFolders.FirstOrDefault(a => a.Slug == slug && a.Parent == parent.Id);
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
            TutorialFolder? current = GetFolder(parentId);
            if (current is null)
            {
                break;
            }

            folderStack.Push(current);
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

        TutorialFolder? folder = GetFolder(article.Folder);
        if (folder is null)
        {
            return article.Slug;
        }

        return $"{GetFullSlug(folder)}/{article.Slug}";
    }

    /// <summary>
    ///     Gets the number of legacy comments for the specified article.
    /// </summary>
    /// <param name="article">The article whose legacy comments to count.</param>
    /// <returns>The total number of legacy comments.</returns>
    public int GetLegacyCommentCount(TutorialArticle article)
    {
        if (article.RedirectFrom is not { } postId)
        {
            return 0;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == postId);
    }

    /// <summary>
    ///     Gets the legacy comments for the specified article.
    /// </summary>
    /// <param name="article">The article whose legacy comments to retrieve.</param>
    /// <returns>A read-only view of the legacy comments.</returns>
    public IReadOnlyList<LegacyComment> GetLegacyComments(TutorialArticle article)
    {
        if (article.RedirectFrom is not { } postId)
        {
            return ArraySegment<LegacyComment>.Empty;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.PostId == postId && c.ParentComment == null)];
    }

    /// <summary>
    ///     Gets the replies to the specified legacy comment.
    /// </summary>
    /// <param name="comment">The comment whose replies to retrieve.</param>
    /// <returns>A read-only view of the replies.</returns>
    public IReadOnlyList<LegacyComment> GetLegacyReplies(LegacyComment comment)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.ParentComment == comment.Id)];
    }

    /// <summary>
    ///     Renders the body of the specified article.
    /// </summary>
    /// <param name="article">The article to render.</param>
    /// <returns>The rendered HTML of the article.</returns>
    public string RenderArticle(TutorialArticle article)
    {
        return Markdig.Markdown.ToHtml(article.Body, _markdownPipeline);
    }

    /// <summary>
    ///     Renders the excerpt of the specified article.
    /// </summary>
    /// <param name="article">The article whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered HTML of the article's excerpt.</returns>
    public string RenderExcerpt(TutorialArticle article, out bool wasTrimmed)
    {
        if (!string.IsNullOrWhiteSpace(article.Excerpt))
        {
            wasTrimmed = false;
            return Markdig.Markdown.ToHtml(article.Excerpt, _markdownPipeline);
        }

        string body = article.Body;
        int moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            string excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return Markdig.Markdown.ToHtml(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return Markdig.Markdown.ToHtml(body[..moreIndex], _markdownPipeline);
    }

    /// <summary>
    ///     Renders the plain text excerpt of the specified article.
    /// </summary>
    /// <param name="article">The article whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered plain text of the article's excerpt.</returns>
    public string RenderPlainTextExcerpt(TutorialArticle article, out bool wasTrimmed)
    {
        if (!string.IsNullOrWhiteSpace(article.Excerpt))
        {
            wasTrimmed = false;
            return Markdig.Markdown.ToPlainText(article.Excerpt, _markdownPipeline);
        }

        string body = article.Body;
        int moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            string excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return Markdig.Markdown.ToPlainText(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return Markdig.Markdown.ToPlainText(body[..moreIndex], _markdownPipeline);
    }

    /// <summary>
    ///     Renders the table of contents for the specified article.
    /// </summary>
    /// <param name="article">The article for which to render the table of contents.</param>
    /// <param name="request">The HTTP request.</param>
    /// <returns>The rendered HTML of the table of contents.</returns>
    public string RenderTableOfContents(TutorialArticle article, HttpRequest request)
    {
        if (article is null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        List<TocItem> items = MarkdownTocBuilder.BuildToc(article.Body);
        return MarkdownTocBuilder.RenderTocAsHtml(items, request);
    }

    /// <summary>
    ///     Attempts to find an article by its ID.
    /// </summary>
    /// <param name="id">The ID of the article.</param>
    /// <param name="article">
    ///     When this method returns, contains the article whose ID matches the specified <paramref name="id" />, or
    ///     <see langword="null" /> if no such article was found.
    /// </param>
    /// <returns><see langword="true" /> if a matching article was found; otherwise, <see langword="false" />.</returns>
    public bool TryGetArticle(Guid id, [NotNullWhen(true)] out TutorialArticle? article)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        article = context.TutorialArticles.FirstOrDefault(a => a.Id == id);
        return article is not null;
    }

    /// <summary>
    ///     Attempts to find an article by its slug.
    /// </summary>
    /// <param name="slug">The slug of the article.</param>
    /// <param name="article">
    ///     When this method returns, contains the article whose slug matches the specified <paramref name="slug" />, or
    ///     <see langword="null" /> if no such article was found.
    /// </param>
    /// <returns><see langword="true" /> if a matching article was found; otherwise, <see langword="false" />.</returns>
    public bool TryGetArticle(string slug, [NotNullWhen(true)] out TutorialArticle? article)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (slug is null)
        {
            article = null;
            return false;
        }

        string[] tokens = slug.Split('/');
        TutorialFolder? folder = null;

        for (var index = 0; index < tokens.Length - 1; index++)
        {
            folder = GetFolder(tokens[index], folder);
        }

        if (folder is null)
        {
            article = null;
            return false;
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        slug = tokens[^1];
        article = context.TutorialArticles.FirstOrDefault(a => a.Slug == slug && a.Folder == folder.Id);
        return article is not null;
    }
}
