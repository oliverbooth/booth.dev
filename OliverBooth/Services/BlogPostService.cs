using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Common.Data;
using OliverBooth.Common.Data.Blog;
using OliverBooth.Common.Services;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogPostService" />.
/// </summary>
internal sealed class BlogPostService : IBlogPostService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly IBlogUserService _blogUserService;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    /// <param name="blogUserService">The <see cref="IBlogUserService" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public BlogPostService(IDbContextFactory<BlogContext> dbContextFactory,
        IBlogUserService blogUserService,
        MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _blogUserService = blogUserService;
        _markdownPipeline = markdownPipeline;
    }

    /// <inheritdoc />
    public int GetBlogPostCount(Visibility visibility = Visibility.None, string[]? tags = null)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        if (tags is { Length: > 0 })
        {
            for (var index = 0; index < tags.Length; index++)
            {
                string tag = tags[index];
                tags[index] = tag.Replace('+', '-');
            }

            return visibility == Visibility.None
                ? context.BlogPosts.AsEnumerable().Count(p => !p.IsRedirect && p.Tags.Intersect(tags).Any())
                : context.BlogPosts.AsEnumerable().Count(p => !p.IsRedirect && p.Visibility == visibility && p.Tags.Intersect(tags).Any());
        }

        return visibility == Visibility.None
            ? context.BlogPosts.Count(p => !p.IsRedirect)
            : context.BlogPosts.Count(p => !p.IsRedirect && p.Visibility == visibility);
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetAllBlogPosts(int limit = -1)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        IQueryable<BlogPost> ordered = context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderByDescending(post => post.Published);
        if (limit > -1)
        {
            ordered = ordered.Take(limit);
        }

        return ordered.AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = IBlogPostService.DefaultPageSize, string[]? tags = null)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        IEnumerable<BlogPost> posts = context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderByDescending(post => post.Published)
            .AsEnumerable();

        if (tags is { Length: > 0 })
        {
            for (var index = 0; index < tags.Length; index++)
            {
                string tag = tags[index];
                tags[index] = tag.Replace('+', '-');
            }

            posts = posts.Where(p => p.Tags.Intersect(tags).Any());
        }

        return posts.Skip(page * pageSize)
            .Take(pageSize)
            .AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public int GetLegacyCommentCount(IBlogPost post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == post.Id);
    }

    /// <inheritdoc />
    public IReadOnlyList<ILegacyComment> GetLegacyComments(IBlogPost post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.PostId == post.Id && c.ParentComment == null).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<ILegacyComment> GetLegacyReplies(ILegacyComment comment)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.ParentComment == comment.Id).ToArray();
    }

    /// <inheritdoc />
    public IBlogPost? GetNextPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderBy(post => post.Published)
            .FirstOrDefault(post => post.Published > blogPost.Published);
    }

    /// <inheritdoc />
    public int GetPageCount(int pageSize = IBlogPostService.DefaultPageSize, Visibility visibility = Visibility.None, string[]? tags = null)
    {
        float postCount = GetBlogPostCount(visibility, tags);
        return (int)MathF.Ceiling(postCount / pageSize);
    }

    /// <inheritdoc />
    public IBlogPost? GetPreviousPost(IBlogPost blogPost)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderByDescending(post => post.Published)
            .FirstOrDefault(post => post.Published < blogPost.Published);
    }

    /// <inheritdoc />
    public string RenderExcerpt(IBlogPost post, out bool wasTrimmed)
    {
        if (!string.IsNullOrWhiteSpace(post.Excerpt))
        {
            wasTrimmed = false;
            return Markdig.Markdown.ToHtml(post.Excerpt, _markdownPipeline);
        }

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

        if (_blogUserService.TryGetUser(post.AuthorId, out IUser? user) && user is IBlogAuthor author)
        {
            post.Author = author;
        }

        return post;
    }
}
