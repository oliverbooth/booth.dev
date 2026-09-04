using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for retrieving legacy comments on blog posts.
/// </summary>
public sealed class CommentService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommentService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The factory for creating instances of <see cref="AppDbContext" />.</param>
    public CommentService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }


    /// <summary>
    ///     Returns the number of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to count.</param>
    /// <returns>The total number of legacy comments.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public int GetLegacyCommentCount(BlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == post.Id);
    }

    /// <summary>
    ///     Gets the number of legacy comments for the specified article.
    /// </summary>
    /// <param name="article">The article whose legacy comments to count.</param>
    /// <returns>The total number of legacy comments.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="article" /> is <see langword="null" />.</exception>
    public int GetLegacyCommentCount(TutorialArticle article)
    {
        if (article is null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        if (article.RedirectFrom is not { } postId)
        {
            return 0;
        }

        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == postId);
    }

    /// <summary>
    ///     Returns the collection of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to retrieve.</param>
    /// <returns>A read-only view of the legacy comments.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public IReadOnlyList<LegacyComment> GetLegacyComments(BlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.PostId == post.Id && c.ParentComment == null)];
    }

    /// <summary>
    ///     Gets the legacy comments for the specified article.
    /// </summary>
    /// <param name="article">The article whose legacy comments to retrieve.</param>
    /// <returns>A read-only view of the legacy comments.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="article" /> is <see langword="null" />.</exception>
    public IReadOnlyList<LegacyComment> GetLegacyComments(TutorialArticle article)
    {
        if (article is null)
        {
            throw new ArgumentNullException(nameof(article));
        }

        if (article.RedirectFrom is not { } postId)
        {
            return [];
        }

        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.PostId == postId && c.ParentComment == null)];
    }

    /// <summary>
    ///     Returns the collection of replies to the specified legacy comment.
    /// </summary>
    /// <param name="comment">The comment whose replies to retrieve.</param>
    /// <returns>A read-only view of the replies.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comment" /> is <see langword="null" />.</exception>
    public IReadOnlyList<LegacyComment> GetLegacyReplies(LegacyComment comment)
    {
        if (comment is null)
        {
            throw new ArgumentNullException(nameof(comment));
        }

        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.ParentComment == comment.Id)];
    }
}
