using System.Collections.Concurrent;
using System.Timers;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a union type for blog post identifiers, which can be a <see cref="Guid" />, an <see cref="int" />, or a
///     <see cref="string" />.
/// </summary>
public union BlogPostKey(Guid, int, string);

/// <summary>
///     Represents an implementation of <see cref="BlogPostService" />.
/// </summary>
public sealed class BlogPostService : BackgroundService
{
    /// <summary>
    ///     The default page size for blog post pagination.
    /// </summary>
    public const int DefaultPageSize = 5;

    private static readonly Timer CacheInvalidationTimer = new(TimeSpan.FromMinutes(10));
    private readonly ILogger<BlogPostService> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly UserService _userService;
    private readonly ConcurrentDictionary<Guid, BlogPost> _postCache = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" />.</param>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="AppDbContext" />.
    /// </param>
    /// <param name="userService">The <see cref="UserService" />.</param>
    public BlogPostService(ILogger<BlogPostService> logger,
        IDbContextFactory<AppDbContext> dbContextFactory,
        UserService userService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _userService = userService;
    }

    /// <summary>
    ///     Returns a collection of all blog posts.
    /// </summary>
    /// <param name="limit">The maximum number of posts to return. A value of -1 returns all posts.</param>
    /// <param name="visibility">The visibility filter for the posts.</param>
    /// <returns>A collection of all blog posts.</returns>
    /// <remarks>
    ///     This method may slow down execution if there are a large number of blog posts being requested. It is
    ///     recommended to use <see cref="GetBlogPosts" /> instead.
    /// </remarks>
    public IReadOnlyList<BlogPost> GetAllBlogPosts(int limit = -1, Visibility visibility = Visibility.Published)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var posts = context.BlogPosts.AsQueryable();
        if (visibility != Visibility.None)
        {
            posts = posts.Where(p => p.Visibility == visibility);
        }

        posts = posts.OrderByDescending(post => post.Published);
        if (limit > -1)
        {
            posts = posts.Take(limit);
        }

        return [.. posts.AsEnumerable().Select(CacheAuthor)];
    }

    /// <summary>
    ///     Returns the total number of blog posts.
    /// </summary>
    /// <param name="visibility">The post visibility filter.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>The total number of blog posts.</returns>
    public int GetBlogPostCount(Visibility visibility = Visibility.None, string[]? tags = null)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        if (tags is { Length: > 0 })
        {
            for (var index = 0; index < tags.Length; index++)
            {
                string tag = tags[index];
                tags[index] = tag.Replace('+', '-');
            }

            return visibility == Visibility.None
                ? context.BlogPosts.AsEnumerable().Count(p => !p.IsRedirect && p.Tags.Intersect(tags).Any())
                : context.BlogPosts.AsEnumerable().Count(p =>
                    !p.IsRedirect && p.Visibility == visibility && p.Tags.Intersect(tags).Any());
        }

        return visibility == Visibility.None
            ? context.BlogPosts.Count(p => !p.IsRedirect)
            : context.BlogPosts.Count(p => !p.IsRedirect && p.Visibility == visibility);
    }

    /// <summary>
    ///     Returns a collection of blog posts from the specified page, optionally limiting the number of posts
    ///     returned per page.
    /// </summary>
    /// <param name="page">The zero-based index of the page to return.</param>
    /// <param name="pageSize">The maximum number of posts to return per page.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>A collection of blog posts.</returns>
    public IReadOnlyList<BlogPost> GetBlogPosts(int page, int pageSize = DefaultPageSize, string[]? tags = null)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        IEnumerable<BlogPost> posts = context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderByDescending(post => post.Published);

        if (tags is { Length: > 0 })
        {
            for (var index = 0; index < tags.Length; index++)
            {
                string tag = tags[index];
                tags[index] = tag.Replace('+', '-');
            }

            posts = posts.AsEnumerable().Where(p => p.Tags.Intersect(tags).Any());
        }

        posts = posts.Skip(page * pageSize).Take(pageSize);
        return [.. posts.AsEnumerable().Select(CacheAuthor)];
    }

    /// <summary>
    ///     Returns the blog post category with the specified ID.
    /// </summary>
    /// <param name="categoryId">The ID of the category to return.</param>
    /// <returns>The blog post category with the specified ID.</returns>
    public BlogPostCategory? GetCategory(Guid categoryId)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPostCategories.Find(categoryId);
    }

    /// <summary>
    ///     Returns the number of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to count.</param>
    /// <returns>The total number of legacy comments.</returns>
    public int GetLegacyCommentCount(BlogPost post)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.LegacyComments.Count(c => c.PostId == post.Id);
    }

    /// <summary>
    ///     Returns the collection of legacy comments for the specified post.
    /// </summary>
    /// <param name="post">The post whose legacy comments to retrieve.</param>
    /// <returns>A read-only view of the legacy comments.</returns>
    public IReadOnlyList<LegacyComment> GetLegacyComments(BlogPost post)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.PostId == post.Id && c.ParentComment == null)];
    }

    /// <summary>
    ///     Returns the collection of replies to the specified legacy comment.
    /// </summary>
    /// <param name="comment">The comment whose replies to retrieve.</param>
    /// <returns>A read-only view of the replies.</returns>
    public IReadOnlyList<LegacyComment> GetLegacyReplies(LegacyComment comment)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.LegacyComments.Where(c => c.ParentComment == comment.Id)];
    }

    /// <summary>
    ///     Returns the next blog post from the specified blog post.
    /// </summary>
    /// <param name="blogPost">The blog post whose next post to return.</param>
    /// <returns>The next blog post from the specified blog post.</returns>
    public BlogPost? GetNextPost(BlogPost blogPost)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderBy(post => post.Published)
            .FirstOrDefault(post => post.Published > blogPost.Published);
    }

    /// <summary>
    ///     Returns the number of pages needed to render all blog posts, using the specified <paramref name="pageSize" /> as an
    ///     indicator of how many posts are allowed per page.
    /// </summary>
    /// <param name="pageSize">The page size. Defaults to 10.</param>
    /// <param name="visibility">The post visibility filter.</param>
    /// <param name="tags">The tags of the posts to return.</param>
    /// <returns>The page count.</returns>
    public int GetPageCount(int pageSize = DefaultPageSize, Visibility visibility = Visibility.None,
        string[]? tags = null)
    {
        float postCount = GetBlogPostCount(visibility, tags);
        return (int)MathF.Ceiling(postCount / pageSize);
    }

    /// <summary>
    ///     Gets the parent-most category of the specified blog post.
    /// </summary>
    /// <param name="post">The blog post whose parent category to retrieve.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the parent-most category of the specified blog post, or an error if the category
    ///     does not exist.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="post" /> is <see langword="null" />.</exception>
    public Result<BlogPostCategory> GetParentCategory(BlogPost post)
    {
        if (post is null)
        {
            throw new ArgumentNullException(nameof(post));
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        BlogPostCategory? current = context.BlogPostCategories.Find(post.CategoryId);

        if (current is null)
        {
            return Result.Fail($"Blog post '{post.Id}' references category '{post.CategoryId}', which does not exist.");
        }

        while (context.BlogPostCategories.Find(current.ParentCategoryId) is { } parent)
        {
            current = parent;
        }

        return current;
    }

    /// <summary>
    ///     Returns the previous blog post from the specified blog post.
    /// </summary>
    /// <param name="blogPost">The blog post whose previous post to return.</param>
    /// <returns>The previous blog post from the specified blog post.</returns>
    public BlogPost? GetPreviousPost(BlogPost blogPost)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts
            .Where(p => p.Visibility == Visibility.Published && !p.IsRedirect)
            .OrderByDescending(post => post.Published)
            .FirstOrDefault(post => post.Published < blogPost.Published);
    }

    /// <summary>
    ///     Returns the blog post with the specified key.
    /// </summary>
    /// <param name="key">The ID or slug of the blog post to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the blog post with the specified ID or slug, or an error if the blog post is not
    ///     found.
    /// </returns>
    public Result<BlogPost> GetPost(BlogPostKey key)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var post = key switch
        {
            Guid guid => context.BlogPosts.Find(guid),
            int intId => context.BlogPosts.FirstOrDefault(p => p.WordPressId == intId),
            string slug => context.BlogPosts.FirstOrDefault(p => p.Slug == slug),
        };

        if (post is null)
        {
            return Result.Fail($"Blog post with the key '{key}' not found.");
        }

        CacheAuthor(post);
        return post;
    }

    /// <summary>
    ///     Returns the blog post with the specified slug that was published on the specified date.
    /// </summary>
    /// <param name="slug">The slug of the blog post to return.</param>
    /// <param name="publishDate">The date the blog post was published.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the blog post with the specified ID or slug, or an error if the blog post is not
    ///     found.
    /// </returns>
    public Result<BlogPost> GetPost(string slug, DateOnly publishDate)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var post = context.BlogPosts.FirstOrDefault(post => post.Published.Year == publishDate.Year &&
                                                            post.Published.Month == publishDate.Month &&
                                                            post.Published.Day == publishDate.Day &&
                                                            post.Slug == slug);

        if (post is null)
        {
            return Result.Fail($"Blog post with slug '{slug}' and date {publishDate} not found.");
        }

        CacheAuthor(post);
        return post;
    }

    /// <summary>
    ///     Returns the most recent blog posts, limited to the specified count.
    /// </summary>
    /// <param name="searchOptions">The options for searching and retrieving blog posts.</param>
    /// <returns>A read-only list of the most recent blog posts.</returns>
    public IReadOnlyList<BlogPost> GetRecentBlogPosts(ActivitySearchOptions searchOptions)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var posts = context.BlogPosts.Where(p => !p.IsRedirect);

        if (searchOptions.Visibility != Visibility.None)
        {
            posts = posts.Where(p => p.Visibility == searchOptions.Visibility);
        }

        var ordered = searchOptions.SortStrategy switch
        {
            ActivitySortStrategy.Published => posts.OrderByDescending(p => p.Published),
            ActivitySortStrategy.Updated => posts.OrderByDescending(p => p.Updated ?? p.Published),
            _ => throw new ArgumentOutOfRangeException(nameof(searchOptions), searchOptions.SortStrategy, "Unknown sort strategy")
        };

        return [.. ordered.Take(searchOptions.Count)];
    }

    /// <summary>
    ///     Returns the top-level blog post categories.
    /// </summary>
    /// <returns>The top-level blog post categories.</returns>
    public BlogPostCategory[] GetTopLevelCategories()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.BlogPostCategories.Where(category => category.ParentCategory == null)];
    }

    /// <summary>
    ///     Searches blog posts for the specified search text.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of blog posts that match the search text.</returns>
    public async Task<IReadOnlyCollection<BlogPost>> SearchBlogPostsAsync(string searchText,
        CancellationToken cancellationToken)
    {
        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        string[] terms =
        [
            .. searchText
                .Split(' ', splitOptions)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
        ];

        if (terms.Length == 0)
        {
            return [];
        }

        const int maxResults = 50;
        var results = new HashSet<BlogPost>(maxResults);

        BlogPost[] posts = [.. _postCache.Values.OrderByDescending(p => p.Published)];
        foreach (BlogPost post in posts)
        {
            if (post.Visibility != Visibility.Published || post.IsRedirect)
            {
                continue;
            }

            bool matches = terms.All(term => post.Title.Contains(term, StringComparison.OrdinalIgnoreCase));

            if (matches)
            {
                results.Add(post);
            }

            if (results.Count >= maxResults)
            {
                break;
            }
        }

        foreach (BlogPost post in posts)
        {
            if (post.Visibility != Visibility.Published || post.IsRedirect)
            {
                continue;
            }

            bool matches = terms.All(term =>
                post.Body.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (post.Excerpt != null && post.Excerpt.Contains(term, StringComparison.OrdinalIgnoreCase)));

            if (matches)
            {
                results.Add(post);
            }

            if (results.Count >= maxResults)
            {
                break;
            }
        }

        return results.AsReadOnly();
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CacheInvalidationTimer.Elapsed += InvalidateCache;
        CacheInvalidationTimer.Start();
        InvalidateCache(this, new ElapsedEventArgs(DateTime.UtcNow));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        CacheInvalidationTimer.Stop();
        CacheInvalidationTimer.Elapsed -= InvalidateCache;
        return base.StopAsync(cancellationToken);
    }

    private void InvalidateCache(object? sender, ElapsedEventArgs e)
    {
        _logger.LogInformation("Invalidating blog post cache...");
        _postCache.Clear();

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        foreach (BlogPost post in context.BlogPosts)
        {
            _postCache[post.Id] = post;
        }

        _logger.LogInformation("Blog post cache invalidated. {PostCount} posts cached.", _postCache.Count);
    }

    private BlogPost CacheAuthor(BlogPost post)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (post.Author is not null)
        {
            return post;
        }

        var result = _userService.GetUser(post.AuthorId);
        if (result.IsSuccess)
        {
            post.Author = result.Value;
        }

        return post;
    }
}
