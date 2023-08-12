using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Blog.Data;

namespace OliverBooth.Blog.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogPostService" />.
/// </summary>
internal sealed class BlogPostService : IBlogPostService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    /// <param name="userService">The <see cref="IUserService" />.</param>
    public BlogPostService(IDbContextFactory<BlogContext> dbContextFactory, IUserService userService)
    {
        _dbContextFactory = dbContextFactory;
        _userService = userService;
    }

    public IReadOnlyList<IBlogPost> GetAllBlogPosts(int limit = -1)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .OrderByDescending(post => post.Published)
            .Take(limit)
            .AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = 10)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .OrderByDescending(post => post.Published)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public string RenderExcerpt(IBlogPost post, out bool wasTrimmed)
    {
        // TODO implement excerpt trimming
        wasTrimmed = false;
        return post.Body;
    }

    /// <inheritdoc />
    public string RenderPost(IBlogPost post)
    {
        // TODO render markdown
        return post.Body;
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
