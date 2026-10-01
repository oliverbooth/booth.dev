using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for retrieving legacy comments on blog posts.
/// </summary>
public sealed class CommentService
{
    private const int MaxAvatarLength = 32767;

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

    /// <summary>
    ///     Returns every legacy comment author, with the avatar they are shown with and how many comments they wrote.
    /// </summary>
    /// <returns>The authors, ordered by name.</returns>
    public IReadOnlyList<LegacyAuthor> GetLegacyAuthors()
    {
        using var context = _dbContextFactory.CreateDbContext();
        return
        [
            .. context.LegacyComments
                .GroupBy(c => c.Author)
                .OrderBy(g => g.Key)
                .Select(g => new LegacyAuthor(g.Key, g.Select(c => c.Avatar).FirstOrDefault(a => a != null), g.Count()))
        ];
    }

    /// <summary>
    ///     Returns every legacy comment written by the specified author.
    /// </summary>
    /// <param name="author">The name of the author.</param>
    /// <returns>The comments, oldest first.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="author" /> is <see langword="null" />.</exception>
    public IReadOnlyList<LegacyComment> GetLegacyCommentsByAuthor(string author)
    {
        ArgumentNullException.ThrowIfNull(author);

        using var context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.Author == author).OrderBy(c => c.CreatedAt)];
    }

    /// <summary>
    ///     Returns the legacy comment with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <returns>The comment, or <see langword="null" /> if there is no such comment.</returns>
    public LegacyComment? GetLegacyComment(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Find(id);
    }

    /// <summary>
    ///     Updates the author name and body of a legacy comment.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <param name="author">The new author name.</param>
    /// <param name="body">The new body.</param>
    /// <returns><see langword="true" /> if the comment was updated; <see langword="false" /> if it doesn't exist.</returns>
    public bool UpdateLegacyComment(Guid id, string author, string body)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments
            .Where(c => c.Id == id)
            .ExecuteUpdate(u => u.SetProperty(c => c.Author, author).SetProperty(c => c.Body, body)) > 0;
    }

    /// <summary>
    ///     Deletes a legacy comment. A comment that has replies can't be deleted, as it would orphan them.
    /// </summary>
    /// <param name="id">The ID of the comment.</param>
    /// <returns><see langword="true" /> if the comment was deleted; otherwise, <see langword="false" />.</returns>
    public bool DeleteLegacyComment(Guid id)
    {
        using var context = _dbContextFactory.CreateDbContext();
        if (context.LegacyComments.Any(c => c.ParentComment == id))
        {
            return false;
        }

        return context.LegacyComments.Where(c => c.Id == id).ExecuteDelete() > 0;
    }

    /// <summary>
    ///     Renames an author on all of their legacy comments.
    /// </summary>
    /// <param name="author">The current name of the author.</param>
    /// <param name="newName">The new name.</param>
    /// <returns>The number of comments updated.</returns>
    public int RenameLegacyAuthor(string author, string newName)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.Author == author).ExecuteUpdate(u => u.SetProperty(c => c.Author, newName));
    }

    /// <summary>
    ///     Sets, or clears, the avatar on all of an author's legacy comments.
    /// </summary>
    /// <param name="author">The name of the author.</param>
    /// <param name="avatar">The new avatar, or <see langword="null" /> to fall back to a generated one.</param>
    /// <returns>The number of comments updated.</returns>
    public int SetLegacyAuthorAvatar(string author, string? avatar)
    {
        using var context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Where(c => c.Author == author).ExecuteUpdate(u => u.SetProperty(c => c.Avatar, avatar));
    }

    /// <summary>
    ///     Encodes an uploaded image as a square PNG <c>data:</c> URI small enough to store in the avatar column.
    /// </summary>
    /// <param name="source">A stream containing the image.</param>
    /// <returns>The data URI, or <see langword="null" /> if the stream isn't a readable image.</returns>
    public static async Task<string?> EncodeAvatarAsync(Stream source)
    {
        Image image;
        try
        {
            image = await Image.LoadAsync(source);
        }
        catch (Exception e) when (e is UnknownImageFormatException or InvalidImageContentException)
        {
            return null;
        }

        using (image)
        {
            foreach (var size in (int[]) [128, 96, 64, 48])
            {
                using var square = image.Clone(c => c.Resize(new ResizeOptions {Size = new Size(size, size), Mode = ResizeMode.Crop}));
                using var buffer = new MemoryStream();
                await square.SaveAsPngAsync(buffer, new PngEncoder {CompressionLevel = PngCompressionLevel.BestCompression});

                var uri = $"data:image/png;base64,{Convert.ToBase64String(buffer.GetBuffer(), 0, (int)buffer.Length)}";
                if (uri.Length <= MaxAvatarLength)
                {
                    return uri;
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Represents a legacy comment author.
    /// </summary>
    /// <param name="Name">The author's name.</param>
    /// <param name="Avatar">The avatar the author's comments carry, if any.</param>
    /// <param name="CommentCount">The number of comments the author wrote.</param>
    public sealed record LegacyAuthor(string Name, string? Avatar, int CommentCount);
}
