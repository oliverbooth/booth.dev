namespace BoothDotDev.Common.Data.Blog;

/// <summary>
///     Represents a blog post being edited.
/// </summary>
public sealed class BlogPostEditModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostEditModel" /> class.
    /// </summary>
    public BlogPostEditModel()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogPostEditModel" /> class.
    /// </summary>
    /// <param name="blogPost">The blog post to edit.</param>
    public BlogPostEditModel(IBlogPost blogPost)
    {
        Id = blogPost.Id;
        AuthorId = blogPost.Author.Id;
        Slug = blogPost.Slug;
        Title = blogPost.Title;
        Body = blogPost.Body;
        Excerpt = blogPost.Excerpt;
        EnableComments = blogPost.EnableComments;
        Password = blogPost.Password;
        Published = blogPost.Published;
        Visibility = blogPost.Visibility;
        IsRedirect = blogPost.IsRedirect;
        RedirectUrl = blogPost.RedirectUrl?.ToString();
    }

    /// <summary>
    ///     Gets or sets the ID of the author of the blog post.
    /// </summary>
    /// <value>The ID of the author of the blog post.</value>
    public Guid AuthorId { get; set; }

    /// <summary>
    ///     Gets or sets the body of the blog post.
    /// </summary>
    /// <value>The body of the blog post.</value>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether comments are enabled for the blog post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if comments are enabled for the blog post; otherwise, <see langword="false" />.
    /// </value>
    public bool EnableComments { get; set; } = true;

    /// <summary>
    ///     Gets or sets the excerpt of the blog post.
    /// </summary>
    /// <value>The excerpt of the blog post.</value>
    public string? Excerpt { get; set; }

    /// <summary>
    ///     Gets the ID of the blog post.
    /// </summary>
    /// <value>The ID of the blog post.</value>
    public Guid Id { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the blog post is a redirect.
    /// </summary>
    /// <value>><see langword="true" /> if the blog post is a redirect; otherwise, <see langword="false" />.</value>
    public bool IsRedirect { get; set; }

    /// <summary>
    ///     Gets or sets the redirect URL of the blog post.
    /// </summary>
    /// <value>The redirect URL of the blog post.</value>
    public string? RedirectUrl { get; set; }

    /// <summary>
    ///     Gets or sets the password of the blog post.
    /// </summary>
    /// <value>The password of the blog post.</value>
    public string? Password
    {
        get => field;
        set => field = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>
    ///     Gets or sets the publication date and time of the blog post.
    /// </summary>
    /// <value>The publication date and time of the blog post.</value>
    public DateTimeOffset Published { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     Gets or sets the slug of the blog post.
    /// </summary>
    /// <value>The slug of the blog post.</value>
    public string Slug { get; set; } = "new-post";

    /// <summary>
    ///     Gets or sets the title of the blog post.
    /// </summary>
    /// <value>The title of the blog post.</value>
    public string Title { get; set; } = "New Post";

    /// <summary>
    ///     Gets or sets the visibility of the blog post.
    /// </summary>
    /// <value>The visibility of the blog post.</value>
    public Visibility Visibility { get; set; } = Visibility.Unlisted;
}
