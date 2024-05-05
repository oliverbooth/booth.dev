using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Data.Web;
using OliverBooth.Common.Services;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

internal sealed class TutorialService : ITutorialService
{
    private readonly IDbContextFactory<BlogContext> _blogContextFactory;
    private readonly IDbContextFactory<WebContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TutorialService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="WebContext" /> factory.</param>
    /// <param name="blogContextFactory">The <see cref="BlogContext" /> factory.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public TutorialService(IDbContextFactory<WebContext> dbContextFactory,
        IDbContextFactory<BlogContext> blogContextFactory,
        MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
        _blogContextFactory = blogContextFactory;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ITutorialArticle> GetArticles(ITutorialFolder folder,
        Visibility visibility = Visibility.None)
    {
        if (folder is null) throw new ArgumentNullException(nameof(folder));

        using WebContext context = _dbContextFactory.CreateDbContext();
        IQueryable<TutorialArticle> articles = context.TutorialArticles.Where(a => a.Folder == folder.Id);

        if (visibility != Visibility.None) articles = articles.Where(a => a.Visibility == visibility);
        return articles.ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ITutorialFolder> GetFolders(ITutorialFolder? parent = null,
        Visibility visibility = Visibility.None)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        IQueryable<TutorialFolder> folders = context.TutorialFolders;

        folders = parent is null ? folders.Where(f => f.Parent == null) : folders.Where(f => f.Parent == parent.Id);
        if (visibility != Visibility.None) folders = folders.Where(a => a.Visibility == visibility);
        return folders.ToArray();
    }

    /// <inheritdoc />
    public ITutorialFolder? GetFolder(int id)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return context.TutorialFolders.FirstOrDefault(f => f.Id == id);
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull(nameof(slug))]
    public ITutorialFolder? GetFolder(string? slug, ITutorialFolder? parent = null)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        return parent is null
            ? context.TutorialFolders.FirstOrDefault(a => a.Slug == slug)
            : context.TutorialFolders.FirstOrDefault(a => a.Slug == slug && a.Parent == parent.Id);
    }

    /// <inheritdoc />
    public string GetFullSlug(ITutorialFolder folder)
    {
        if (folder is null) throw new ArgumentNullException(nameof(folder));

        var folderStack = new Stack<ITutorialFolder>();
        folderStack.Push(folder);

        while (folder.Parent is { } parentId)
        {
            ITutorialFolder? current = GetFolder(parentId);
            if (current is null) break;
            folderStack.Push(current);
        }

        using var builder = ZString.CreateUtf8StringBuilder();

        while (folderStack.Count > 0)
        {
            builder.Append(folderStack.Pop().Slug);

            if (folderStack.Count > 0)
                builder.Append('/');
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    public string GetFullSlug(ITutorialArticle article)
    {
        if (article is null) throw new ArgumentNullException(nameof(article));
        ITutorialFolder? folder = GetFolder(article.Folder);
        if (folder is null) return article.Slug;
        return $"{GetFullSlug(folder)}/{article.Slug}";
    }

    /// <inheritdoc />
    public int GetLegacyCommentCount(ITutorialArticle article)
    {
        if (article.RedirectFrom is not { } postId)
        {
            return 0;
        }

        using BlogContext context = _blogContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == postId);
    }

    /// <inheritdoc />
    public IReadOnlyList<ILegacyComment> GetLegacyComments(ITutorialArticle article)
    {
        if (article.RedirectFrom is not { } postId)
        {
            return ArraySegment<ILegacyComment>.Empty;
        }

        using BlogContext context = _blogContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.PostId == postId && c.ParentComment == null).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<ILegacyComment> GetLegacyReplies(ILegacyComment comment)
    {
        using BlogContext context = _blogContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.ParentComment == comment.Id).ToArray();
    }

    /// <inheritdoc />
    public string RenderArticle(ITutorialArticle article)
    {
        return Markdig.Markdown.ToHtml(article.Body, _markdownPipeline);
    }

    /// <inheritdoc />
    public string RenderExcerpt(ITutorialArticle article, out bool wasTrimmed)
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

    /// <inheritdoc />
    public bool TryGetArticle(int id, [NotNullWhen(true)] out ITutorialArticle? article)
    {
        using WebContext context = _dbContextFactory.CreateDbContext();
        article = context.TutorialArticles.FirstOrDefault(a => a.Id == id);
        return article is not null;
    }

    /// <inheritdoc />
    public bool TryGetArticle(string slug, [NotNullWhen(true)] out ITutorialArticle? article)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (slug is null)
        {
            article = null;
            return false;
        }

        string[] tokens = slug.Split('/');
        ITutorialFolder? folder = null;

        for (var index = 0; index < tokens.Length - 1; index++)
        {
            folder = GetFolder(tokens[index], folder);
        }

        if (folder is null)
        {
            article = null;
            return false;
        }

        using WebContext context = _dbContextFactory.CreateDbContext();
        slug = tokens[^1];
        article = context.TutorialArticles.FirstOrDefault(a => a.Slug == slug && a.Folder == folder.Id);
        return article is not null;
    }
}
