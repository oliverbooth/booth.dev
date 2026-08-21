using System.Collections.Concurrent;
using System.Timers;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a key used to identify a blog post either by its ID, its legacy WordPress ID, or its URL slug.
/// </summary>
public union BlogPostKey(Guid, int, string);

/// <summary>
///     Represents a service for retrieving and managing blog posts.
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
    ///     Creates a new blog post, along with its first draft, which immediately becomes the post's current draft.
    /// </summary>
    /// <param name="authorId">The ID of the author creating the post.</param>
    /// <param name="title">The title of the post.</param>
    /// <param name="slug">The URL slug of the post.</param>
    /// <param name="body">The body of the post.</param>
    /// <param name="excerpt">The excerpt of the post, if any.</param>
    /// <param name="categoryId">The ID of the post's category.</param>
    /// <param name="visibility">The visibility of the post.</param>
    /// <param name="publishedAt">The publication date and time of the post.</param>
    /// <param name="tags">The tags associated with the post.</param>
    /// <returns>A <see cref="Result{T}" /> containing the newly-created blog post.</returns>
    public Result<BlogPost> CreatePost(Guid authorId,
        string title,
        string slug,
        string body,
        string? excerpt,
        Guid categoryId,
        Visibility visibility,
        DateTimeOffset publishedAt,
        IReadOnlyList<string> tags)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();

        var post = new BlogPost
        {
            AuthorId = authorId,
            Slug = slug,
            Published = publishedAt.ToUniversalTime(),
            Updated = null
        };

        var draft = NewDraft(post.Id, title, body, excerpt, categoryId, visibility, tags);
        post.CurrentDraft = draft;

        context.BlogPosts.Add(post);
        context.BlogPostDrafts.Add(draft);
        context.SaveChanges();

        _postCache[post.Id] = post;
        return post;
    }

    /// <summary>
    ///     Saves a new draft of an existing blog post, without publishing it. The post's current draft is left
    ///     unchanged, so the public site is unaffected.
    /// </summary>
    /// <param name="id">The ID of the post to save a draft for.</param>
    /// <param name="title">The title of the post.</param>
    /// <param name="body">The body of the post.</param>
    /// <param name="excerpt">The excerpt of the post, if any.</param>
    /// <param name="categoryId">The ID of the post's category.</param>
    /// <param name="visibility">The visibility of the post.</param>
    /// <param name="tags">The tags associated with the post.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the post the draft was saved for, or an error if no post with the
    ///     specified <paramref name="id" /> exists.
    /// </returns>
    public Result<BlogPost> SaveDraft(Guid id,
        string title,
        string body,
        string? excerpt,
        Guid categoryId,
        Visibility visibility,
        IReadOnlyList<string> tags)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var post = context.BlogPosts.Find(id);

        if (post is null)
        {
            return Result.Fail($"Blog post with ID '{id}' not found.");
        }

        var draft = NewDraft(post.Id, title, body, excerpt, categoryId, visibility, tags);
        context.BlogPostDrafts.Add(draft);
        context.SaveChanges();

        _postCache[post.Id] = post;
        return post;
    }

    /// <summary>
    ///     Saves a new draft of an existing blog post and publishes it, making it the post's current draft.
    /// </summary>
    /// <param name="id">The ID of the post to publish.</param>
    /// <param name="title">The title of the post.</param>
    /// <param name="slug">The URL slug of the post.</param>
    /// <param name="body">The body of the post.</param>
    /// <param name="excerpt">The excerpt of the post, if any.</param>
    /// <param name="categoryId">The ID of the post's category.</param>
    /// <param name="visibility">The visibility of the post.</param>
    /// <param name="publishedAt">The publication date and time of the post.</param>
    /// <param name="tags">The tags associated with the post.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the updated blog post, or an error if no post with the specified
    ///     <paramref name="id" /> exists.
    /// </returns>
    public Result<BlogPost> PublishPost(Guid id,
        string title,
        string slug,
        string body,
        string? excerpt,
        Guid categoryId,
        Visibility visibility,
        DateTimeOffset publishedAt,
        IReadOnlyList<string> tags)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var post = context.BlogPosts.Find(id);

        if (post is null)
        {
            return Result.Fail($"Blog post with ID '{id}' not found.");
        }

        var draft = NewDraft(post.Id, title, body, excerpt, categoryId, visibility, tags);
        context.BlogPostDrafts.Add(draft);

        post.Slug = slug;
        post.Published = publishedAt.ToUniversalTime();
        post.CurrentDraft = draft;
        post.Updated = DateTimeOffset.UtcNow;

        context.SaveChanges();

        _postCache[post.Id] = post;
        return post;
    }

    /// <summary>
    ///     Returns a collection of all blog posts.
    /// </summary>
    /// <param name="limit">The maximum number of posts to return. A value of -1 returns all posts.</param>
    /// <param name="visibility">The visibility filter for the posts.</param>
    /// <returns>A collection of all blog posts.</returns>
    public IReadOnlyList<BlogPost> GetAllBlogPosts(int limit = -1, Visibility visibility = Visibility.Published)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var posts = context.BlogPosts.Include(p => p.CurrentDraft).AsQueryable();
        if (visibility != Visibility.None)
        {
            posts = posts.Where(p => p.CurrentDraft!.Visibility == visibility);
        }

        posts = posts.OrderByDescending(post => post.Published);
        if (limit > -1)
        {
            posts = posts.Take(limit);
        }

        return [.. posts.AsEnumerable().Select(CacheAuthor)];
    }

    /// <summary>
    ///     Returns a collection of all blog post categories, including their child categories.
    /// </summary>
    /// <returns>A read-only list of all blog post categories with their child categories.</returns>
    public IReadOnlyList<BlogPostCategory> GetAllCategories()
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var all = context.BlogPostCategories.ToList();
        var byParent = all.ToLookup(c => c.ParentCategoryId);

        foreach (var category in all)
        {
            category.Children = [.. byParent[category.Id]];
        }

        return [.. all.Where(c => c.ParentCategoryId is null)];
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
        var posts = context.BlogPosts.Include(p => p.CurrentDraft).AsQueryable();

        if (tags is { Length: > 0 })
        {
            for (var index = 0; index < tags.Length; index++)
            {
                string tag = tags[index];
                tags[index] = tag.Replace('+', '-');
            }

            return visibility == Visibility.None
                ? posts.AsEnumerable().Count(p => !p.IsRedirect && p.CurrentDraft!.Tags.Intersect(tags).Any())
                : posts.AsEnumerable().Count(p =>
                    !p.IsRedirect && p.CurrentDraft!.Visibility == visibility && p.CurrentDraft.Tags.Intersect(tags).Any());
        }

        return visibility == Visibility.None
            ? posts.Count(p => !p.IsRedirect)
            : posts.Count(p => !p.IsRedirect && p.CurrentDraft!.Visibility == visibility);
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
    ///     Returns the draft history of the specified blog post, newest first.
    /// </summary>
    /// <param name="id">The ID of the post whose draft history to return.</param>
    /// <returns>A read-only list of the post's drafts, ordered newest first.</returns>
    public IReadOnlyList<BlogPostDraft> GetDraftHistory(Guid id)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        return [.. context.BlogPostDrafts.Where(d => d.BlogPostId == id).OrderByDescending(d => d.CreatedAt)];
    }

    /// <summary>
    ///     Returns a specific draft of the specified blog post, for viewing without publishing it.
    /// </summary>
    /// <param name="id">The ID of the post the draft belongs to.</param>
    /// <param name="draftId">The ID of the draft to return.</param>
    /// <returns>
    ///     A <see cref="Result{T}" /> containing the requested draft, or an error if it doesn't exist or doesn't belong to the
    ///     specified post.
    /// </returns>
    public Result<BlogPostDraft> GetDraft(Guid id, Guid draftId)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var draft = context.BlogPostDrafts.Find(draftId);

        if (draft is null || draft.BlogPostId != id)
        {
            return Result.Fail($"Draft '{draftId}' not found for blog post '{id}'.");
        }

        return draft;
    }

    /// <summary>
    ///     Returns the newest draft of the specified blog post, which may or may not be the post's current (published) draft.
    /// </summary>
    /// <param name="id">The ID of the post whose newest draft to return.</param>
    /// <returns>A <see cref="Result{T}" /> containing the post's newest draft, or an error if the post has no drafts.</returns>
    public Result<BlogPostDraft> GetNewestDraft(Guid id)
    {
        using AppDbContext context = _dbContextFactory.CreateDbContext();
        var draft = context.BlogPostDrafts
            .Where(d => d.BlogPostId == id)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();

        if (draft is null)
        {
            return Result.Fail($"Blog post '{id}' has no drafts.");
        }

        return draft;
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
            .Include(p => p.CurrentDraft)
            .Where(p => p.CurrentDraft!.Visibility == Visibility.Published && !p.IsRedirect)
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
            .Include(p => p.CurrentDraft)
            .Where(p => p.CurrentDraft!.Visibility == Visibility.Published && !p.IsRedirect)
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
        var posts = context.BlogPosts.Include(p => p.CurrentDraft);
        var post = key switch
        {
            Guid guid => posts.FirstOrDefault(p => p.Id == guid),
            int intId => posts.FirstOrDefault(p => p.WordPressId == intId),
            string slug => posts.FirstOrDefault(p => p.Slug == slug),
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
        var post = context.BlogPosts
            .Include(p => p.CurrentDraft)
            .FirstOrDefault(post => post.Published.Year == publishDate.Year &&
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
        var posts = context.BlogPosts.Include(p => p.CurrentDraft).Where(p => !p.IsRedirect);

        if (searchOptions.Visibility != Visibility.None)
        {
            posts = posts.Where(p => p.CurrentDraft!.Visibility == searchOptions.Visibility);
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

    /// <summary>
    ///     Builds a new, unsaved draft snapshot for the specified blog post.
    /// </summary>
    private static BlogPostDraft NewDraft(Guid blogPostId,
        string title,
        string body,
        string? excerpt,
        Guid categoryId,
        Visibility visibility,
        IReadOnlyList<string> tags)
    {
        return new BlogPostDraft
        {
            BlogPostId = blogPostId,
            Title = title,
            Body = body,
            Excerpt = excerpt,
            CategoryId = categoryId,
            Visibility = visibility,
            Tags = [.. tags]
        };
    }

    private void InvalidateCache(object? sender, ElapsedEventArgs e)
    {
        _logger.LogInformation("Invalidating blog post cache...");
        _postCache.Clear();

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        foreach (BlogPost post in context.BlogPosts.Include(p => p.CurrentDraft))
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
