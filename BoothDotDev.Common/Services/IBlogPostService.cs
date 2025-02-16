using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Blog;

namespace BoothDotDev.Common.Services;

/// <summary>
///     Represents a service for managing blog posts.
/// </summary>
public interface IBlogPostService
{
    public const int DefaultPageSize = 5;

    /// <summary>
    ///     Returns a collection of all blog posts.
    /// </summary>
    /// <param name="limit">The maximum number of posts to return. A value of -1 returns all posts.</param>
    /// <returns>A collection of all blog posts.</returns>
    /// <remarks>
    ///     This method may slow down execution if there are a large number of blog posts being requested. It is
    ///     recommended to use <see cref="GetBlogPosts" /> instead.
    /// </remarks>
    IReadOnlyList<IBlogPost> GetAllBlogPosts(int limit = -1);

    /// <summary>
    ///     Returns the total number of blog posts.
    /// </summary>
    /// <param name="visibility">The post visibility filter.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>The total number of blog posts.</returns>
    int GetBlogPostCount(Visibility visibility = Visibility.None, string[]? tags = null);

    /// <summary>
    ///     Returns a collection of blog posts from the specified page, optionally limiting the number of posts
    ///     returned per page.
    /// </summary>
    /// <param name="page">The zero-based index of the page to return.</param>
    /// <param name="pageSize">The maximum number of posts to return per page.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>A collection of blog posts.</returns>
    IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = DefaultPageSize, string[]? tags = null);

    /// <summary>
    ///     Returns the number of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to count.</param>
    /// <returns>The total number of legacy comments.</returns>
    int GetLegacyCommentCount(IBlogPost post);

    /// <summary>
    ///     Returns the collection of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to retrieve.</param>
    /// <returns>A read-only view of the legacy comments.</returns>
    IReadOnlyList<ILegacyComment> GetLegacyComments(IBlogPost post);

    /// <summary>
    ///     Returns the collection of replies to the specified legacy comment.
    /// </summary>
    /// <param name="comment">The comment whose replies to retrieve.</param>
    /// <returns>A read-only view of the replies.</returns>
    IReadOnlyList<ILegacyComment> GetLegacyReplies(ILegacyComment comment);

    /// <summary>
    ///     Returns the next blog post from the specified blog post.
    /// </summary>
    /// <param name="blogPost">The blog post whose next post to return.</param>
    /// <returns>The next blog post from the specified blog post.</returns>
    IBlogPost? GetNextPost(IBlogPost blogPost);

    /// <summary>
    ///     Returns the number of pages needed to render all blog posts, using the specified <paramref name="pageSize" /> as an
    ///     indicator of how many posts are allowed per page.
    /// </summary>
    /// <param name="pageSize">The page size. Defaults to 10.</param>
    /// <param name="visibility">The post visibility filter.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>The page count.</returns>
    int GetPageCount(int pageSize = DefaultPageSize, Visibility visibility = Visibility.None, string[]? tags = null);

    /// <summary>
    ///     Returns the previous blog post from the specified blog post.
    /// </summary>
    /// <param name="blogPost">The blog post whose previous post to return.</param>
    /// <returns>The previous blog post from the specified blog post.</returns>
    IBlogPost? GetPreviousPost(IBlogPost blogPost);

    /// <summary>
    ///     Renders the excerpt of the specified blog post.
    /// </summary>
    /// <param name="post">The blog post whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered HTML of the blog post's excerpt.</returns>
    string RenderExcerpt(IBlogPost post, out bool wasTrimmed);

    /// <summary>
    ///     Renders the excerpt of the specified blog post as plain text.
    /// </summary>
    /// <param name="post">The blog post whose excerpt to render.</param>
    /// <param name="wasTrimmed">
    ///     When this method returns, contains <see langword="true" /> if the excerpt was trimmed; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns>The rendered plain text of the blog post's excerpt.</returns>
    string RenderPlainTextExcerpt(IBlogPost post, out bool wasTrimmed);

    /// <summary>
    ///     Renders the body of the specified blog post.
    /// </summary>
    /// <param name="post">The blog post to render.</param>
    /// <returns>The rendered HTML of the blog post.</returns>
    string RenderPost(IBlogPost post);

    /// <summary>
    ///     Attempts to find a blog post with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the blog post to find.</param>
    /// <param name="post">
    ///     When this method returns, contains the blog post with the specified ID, if the blog post is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a blog post with the specified ID is found; otherwise, <see langword="false" />.
    /// </returns>
    bool TryGetPost(Guid id, [NotNullWhen(true)] out IBlogPost? post);

    /// <summary>
    ///     Attempts to find a blog post with the specified WordPress ID.
    /// </summary>
    /// <param name="id">The ID of the blog post to find.</param>
    /// <param name="post">
    ///     When this method returns, contains the blog post with the specified WordPress ID, if the blog post is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a blog post with the specified WordPress ID is found; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    bool TryGetPost(int id, [NotNullWhen(true)] out IBlogPost? post);

    /// <summary>
    ///     Attempts to find a blog post with the specified publish date and URL slug.
    /// </summary>
    /// <param name="publishDate">The date the blog post was published.</param>
    /// <param name="slug">The URL slug of the blog post to find.</param>
    /// <param name="post">
    ///     When this method returns, contains the blog post with the specified publish date and URL slug, if the blog
    ///     post is found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if a blog post with the specified publish date and URL slug is found; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug" /> is <see langword="null" />.</exception>
    bool TryGetPost(DateOnly publishDate, string slug, [NotNullWhen(true)] out IBlogPost? post);
}
