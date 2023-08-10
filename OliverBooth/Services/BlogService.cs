using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

public sealed class BlogService
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public BlogService(IDbContextFactory<BlogContext> dbContextFactory, MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
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
    ///     Gets the processed content of a blog post.
    /// </summary>
    /// <param name="post">The blog post.</param>
    /// <returns>The processed content of the blog post.</returns>
    public string GetContent(BlogPost post)
    {
        return RenderContent(post.Body);
    }

    /// <summary>
    ///     Gets the processed excerpt of a blog post.
    /// </summary>
    /// <param name="post">The blog post.</param>
    /// <param name="trimmed">
    ///     When this method returns, contains <see langword="true" /> if the content was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The processed excerpt of the blog post.</returns>
    public string GetExcerpt(BlogPost post, out bool trimmed)
    {
        ReadOnlySpan<char> span = post.Body.AsSpan();
        int moreIndex = span.IndexOf("<!--more-->", StringComparison.Ordinal);
        trimmed = moreIndex != -1 || span.Length > 256;
        string result = moreIndex != -1 ? span[..moreIndex].Trim().ToString() : post.Body.Truncate(256);
        return RenderContent(result).Trim();
    }

    /// <summary>
    ///     Attempts to find the author by ID.
    /// </summary>
    /// <param name="id">The ID of the author.</param>
    /// <param name="author">
    ///     When this method returns, contains the <see cref="Author" /> associated with the specified ID, if the author
    ///     is found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the author is found; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public bool TryGetAuthor(int id, [NotNullWhen(true)] out Author? author)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        author = context.Authors.FirstOrDefault(a => a.Id == id);

        return author is not null;
    }

    /// <summary>
    ///     Attempts to find the author of a blog post.
    /// </summary>
    /// <param name="post">The blog post.</param>
    /// <param name="author">
    ///     When this method returns, contains the <see cref="Author" /> associated with the specified blog post, if the
    ///     author is found; otherwise, <see langword="null" />.
    /// </param>
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
    ///     Attempts to find a blog post by new ID.
    /// </summary>
    /// <param name="postId">The new ID of the post.</param>
    /// <param name="post">
    ///     When this method returns, contains the <see cref="BlogPost" /> associated with ID, if the post is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the post is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetBlogPost(int postId, [NotNullWhen(true)] out BlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p => p.Id == postId);
        return post is not null;
    }

    /// <summary>
    ///     Attempts to find a blog post by its legacy WordPress ID.
    /// </summary>
    /// <param name="postId">The WordPress ID of the post.</param>
    /// <param name="post">
    ///     When this method returns, contains the <see cref="BlogPost" /> associated with ID, if the post is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the post is found; otherwise, <see langword="false" />.</returns>
    public bool TryGetWordPressBlogPost(int postId, [NotNullWhen(true)] out BlogPost? post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        post = context.BlogPosts.FirstOrDefault(p => p.WordPressId == postId);
        return post is not null;
    }
    
    private string RenderContent(string content)
    {
        content = content.Replace("<!--more-->", string.Empty);

        while (content.Contains("\n\n"))
        {
            content = content.Replace("\n\n", "\n");
        }

        return Markdig.Markdown.ToHtml(content.Trim(), _markdownPipeline);
    }
}
