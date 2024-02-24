using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogPostService" />.
/// </summary>
internal sealed class BlogPostService : IBlogPostService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly IUserService _userService;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public BlogPostService(IDbContextFactory<BlogContext> dbContextFactory,
        IUserService userService,
        MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _userService = userService;
        _markdownPipeline = markdownPipeline;
    }

    /// <inheritdoc />
    public int GetBlogPostCount()
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts.Count();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetAllBlogPosts(int limit = -1)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        IQueryable<BlogPost> ordered = context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderByDescending(post => post.Published);
        if (limit > -1)
        {
            ordered = ordered.Take(limit);
        }

        return ordered.AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = 10)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderByDescending(post => post.Published)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToArray().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IBlogPost? GetNextPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderBy(post => post.Published)
            .FirstOrDefault(post => post.Published > blogPost.Published);
    }

    /// <inheritdoc />
    public IBlogPost? GetPreviousPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == BlogPostVisibility.Published)
            .OrderByDescending(post => post.Published)
            .FirstOrDefault(post => post.Published < blogPost.Published);
    }

    /// <inheritdoc />
    public string RenderExcerpt(IBlogPost post, out bool wasTrimmed)
    {
        string body = post.Body;
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
    public string RenderPost(IBlogPost post)
    {
        return Markdig.Markdown.ToHtml(post.Body, _markdownPipeline);
    }

    /// <inheritdoc />
    public bool TryGetPost(Guid id, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.Find(id);
        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetPost(int id, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p => p.WordPressId == id);
        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    /// <inheritdoc />
    public bool TryGetPost(DateOnly publishDate, string slug, [NotNullWhen(true)] out IBlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(post => post.Published.Year == publishDate.Year &&
                                                        post.Published.Month == publishDate.Month &&
                                                        post.Published.Day == publishDate.Day &&
                                                        post.Slug == slug);

        if (post is null)
        {
            return false;
        }

        CacheAuthor((BlogPost)post);
        return true;
    }

    private BlogPost CacheAuthor(BlogPost post)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (post.Author is not null)
        {
            return post;
        }

        if (_userService.TryGetUser(post.AuthorId, out IUser? user) && user is IBlogAuthor author)
        {
            post.Author = author;
        }

        return post;
    }
}
