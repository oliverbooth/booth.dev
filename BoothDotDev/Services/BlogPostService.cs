using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Timers;
using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using BoothDotDev.Data.Blog;
using Humanizer;
using Markdig;
using Microsoft.EntityFrameworkCore;
using Timer = System.Timers.Timer;

namespace BoothDotDev.Services;

/// <summary>
///     Represents an implementation of <see cref="IBlogPostService" />.
/// </summary>
internal sealed class BlogPostService : BackgroundService, IBlogPostService
{
    private static readonly Timer CacheInvalidationTimer = new(TimeSpan.FromMinutes(10).TotalMilliseconds);
    private readonly ILogger<BlogPostService> _logger;
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly IBlogUserService _blogUserService;
    private readonly MarkdownPipeline _markdownPipeline;
    private readonly ConcurrentDictionary<Guid, BlogPost> _postCache = [];

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostService" /> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}" />.</param>
    /// <param name="dbContextFactory">
    ///     The <see cref="IDbContextFactory{TContext}" /> used to create a <see cref="BlogContext" />.
    /// </param>
    /// <param name="blogUserService">The <see cref="IBlogUserService" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public BlogPostService(ILogger<BlogPostService> logger,
        IDbContextFactory<BlogContext> dbContextFactory,
        IBlogUserService blogUserService,
        MarkdownPipeline markdownPipeline)
    {
        _logger = logger;
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
    public IReadOnlyList<IBlogPost> GetEverySingleBlogPost()
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.BlogPosts.OrderByDescending(post => post.Published).AsEnumerable().Select(CacheAuthor).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlogPost> GetBlogPosts(int page, int pageSize = IBlogPostService.DefaultPageSize, string[]? tags = null)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
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
        return posts.AsEnumerable().Select(CacheAuthor).ToArray();
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
    public string RenderPlainTextExcerpt(IBlogPost post, out bool wasTrimmed)
    {
        if (!string.IsNullOrWhiteSpace(post.Excerpt))
        {
            wasTrimmed = false;
            return Markdig.Markdown.ToPlainText(post.Excerpt, _markdownPipeline);
        }

        string body = post.Body;
        int moreIndex = body.IndexOf("<!--more-->", StringComparison.Ordinal);

        if (moreIndex == -1)
        {
            string excerpt = body.Truncate(255, "...");
            wasTrimmed = body.Length > 255;
            return Markdig.Markdown.ToPlainText(excerpt, _markdownPipeline);
        }

        wasTrimmed = true;
        return Markdig.Markdown.ToPlainText(body[..moreIndex], _markdownPipeline);
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
    public async Task<IReadOnlyCollection<IBlogPost>> SearchBlogPostsAsync(string searchText, CancellationToken cancellationToken)
    {
        const StringSplitOptions splitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return [];
        }

        string[] terms = searchText
            .Split(' ', splitOptions)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .ToArray();

        if (terms.Length == 0)
        {
            return [];
        }

        const int maxResults = 50;
        var results = new HashSet<BlogPost>(maxResults);

        BlogPost[] posts = _postCache.Values.OrderByDescending(p => p.Published).ToArray();
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

    /// <inheritdoc />
    public void UpdateBlogPost(BlogPostEditModel blogPost)
    {
        if (blogPost is null)
        {
            throw new ArgumentNullException(nameof(blogPost));
        }

        using BlogContext context = _dbContextFactory.CreateDbContext();
        BlogPost? existingPost = context.BlogPosts.Find(blogPost.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException($"Blog post with ID '{blogPost.Id}' not found.");
        }

        existingPost.Slug = blogPost.Slug;
        existingPost.Title = blogPost.Title;
        existingPost.Body = blogPost.Body;
        existingPost.Excerpt = blogPost.Excerpt;
        existingPost.EnableComments = blogPost.EnableComments;
        existingPost.Password = blogPost.Password;
        existingPost.Published = blogPost.Published;
        existingPost.Visibility = blogPost.Visibility;
        existingPost.Updated = DateTimeOffset.UtcNow;
        existingPost.IsRedirect = blogPost.IsRedirect;
        existingPost.RedirectUrl = string.IsNullOrWhiteSpace(blogPost.RedirectUrl) ? null : new Uri(blogPost.RedirectUrl);
        context.Update(existingPost);
        context.SaveChanges();
    }

    /// <inheritdoc />
    public void UpdateBlogPost(Guid id, Action<BlogPostEditModel> updateAction)
    {
        if (updateAction is null)
        {
            throw new ArgumentNullException(nameof(updateAction));
        }

        if (!TryGetPost(id, out IBlogPost? post))
        {
            throw new InvalidOperationException($"Blog post with ID '{id}' not found.");
        }

        var editModel = new BlogPostEditModel(post);
        updateAction(editModel);
        UpdateBlogPost(editModel);
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

        using BlogContext context = _dbContextFactory.CreateDbContext();
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

        if (_blogUserService.TryGetUser(post.AuthorId, out IUser? user) && user is IBlogAuthor author)
        {
            post.Author = author;
        }

        return post;
    }
}
