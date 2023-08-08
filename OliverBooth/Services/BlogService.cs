using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

public sealed class BlogService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    public BlogService(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets a read-only view of all blog posts.
    /// </summary>
    /// <returns>A read-only view of all blog posts.</returns>
    public IReadOnlyCollection<BlogPost> AllPosts
    {
        get
        {
            using BlogContext context = _dbContextFactory.CreateDbContext();
            return context.BlogPosts.OrderByDescending(p => p.Published).ToArray();
        }
    }

    /// <summary>
    ///     Attempts to find the author of a blog post.
    /// </summary>
    /// <param name="post">The blog post.</param>
    /// <param name="author">
    ///     When this method returns, contains the <see cref="Author" /> associated with the specified blog post, if the
    ///     author is found; otherwise, <see langword="null" />.
    /// <returns><see langword="true" /> if the author is found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public bool TryGetAuthor(BlogPost post, [NotNullWhen(true)] out Author? author)
    {
        if (post is null) throw new ArgumentNullException(nameof(post));

        using BlogContext context = _dbContextFactory.CreateDbContext();
        author = context.Authors.FirstOrDefault(a => a.Id == post.AuthorId);

        return author is not null;
    }

    /// <summary>
    ///     Attempts to find a blog post by its publication date and slug.
    /// </summary>
    /// <param name="year">The year the post was published.</param>
    /// <param name="month">The month the post was published.</param>
    /// <param name="day">The day the post was published.</param>
    /// <param name="slug">The slug of the post.</param>
    /// <param name="post">
    ///     When this method returns, contains the <see cref="BlogPost" /> associated with the specified publication
    ///     date and slug, if the post is found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the post is found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    public bool TryGetBlogPost(int year, int month, int day, string slug, [NotNullWhen(true)] out BlogPost? post)
    {
        if (slug is null) throw new ArgumentNullException(nameof(slug));

        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p =>
            p.Published.Year == year && p.Published.Month == month && p.Published.Day == day &&
            p.Slug == slug);

        return post is not null;
    }

    /// <summary>
    ///     Attempts to find a blog post by its publication date and slug.
    /// </summary>
    /// <param name="postId">The WordPress ID of the post.</param>
    /// <param name="post">
    ///     When this method returns, contains the <see cref="BlogPost" /> associated with the specified publication
    ///     date and slug, if the post is found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the post is found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    public bool TryGetWordPressBlogPost(int postId, [NotNullWhen(true)] out BlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p => p.WordPressId == postId);
        return post is not null;
    }
}
