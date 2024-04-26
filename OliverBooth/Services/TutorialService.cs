using System.Diagnostics.CodeAnalysis;
using Cysharp.Text;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

internal sealed class TutorialService : ITutorialService
{
    private readonly IDbContextFactory<WebContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TutorialService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public TutorialService(IDbContextFactory<WebContext> dbContextFactory, MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
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
    public string RenderArticle(ITutorialArticle article)
    {
        return Markdig.Markdown.ToHtml(article.Body, _markdownPipeline);
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
