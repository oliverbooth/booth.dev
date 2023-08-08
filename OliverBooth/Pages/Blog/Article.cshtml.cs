using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OliverBooth.Data.Blog;
using OliverBooth.Services;

namespace OliverBooth.Pages.Blog;

/// <summary>
///     Represents the page model for the <c>Article</c> page.
/// </summary>
public class Article : PageModel
{
    private readonly BlogService _blogService;
    private readonly MarkdownPipeline _markdownPipeline;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Article" /> class.
    /// </summary>
    /// <param name="blogService">The <see cref="BlogService" />.</param>
    /// <param name="markdownPipeline">The <see cref="MarkdownPipeline" />.</param>
    public Article(BlogService blogService, MarkdownPipeline markdownPipeline)
    {
        _blogService = blogService;
        _markdownPipeline = markdownPipeline;
    }

    /// <summary>
    ///     Gets the author of the post.
    /// </summary>
    /// <value>The author of the post.</value>
    public Author Author { get; private set; } = null!;

    /// <summary>
    ///     Gets a value indicating whether the post is a legacy WordPress post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post is a legacy WordPress post; otherwise, <see langword="false" />.
    /// </value>
    public bool IsWordPressLegacyPost => Post.WordPressId.HasValue;

    /// <summary>
    ///     Gets the requested blog post.
    /// </summary>
    /// <value>The requested blog post.</value>
    public BlogPost Post { get; private set; } = null!;

    /// <summary>
    ///     Sanitizes the content of the blog post.
    /// </summary>
    /// <param name="content">The content of the blog post.</param>
    /// <returns>The sanitized content of the blog post.</returns>
    public string SanitizeContent(string content)
    {
        content = content.Replace("<!--more-->", string.Empty);

        while (content.Contains("\n\n"))
        {
            content = content.Replace("\n\n", "\n");
        }

        return Markdown.ToHtml(content.Trim(), _markdownPipeline);
    }

    public IActionResult OnGet(int year, int month, int day, string slug)
    {
        if (!_blogService.TryGetBlogPost(year, month, day, slug, out BlogPost? post))
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        Post = post;
        Author = _blogService.TryGetAuthor(post, out Author? author) ? author : null!;
        return Page();
    }
}
