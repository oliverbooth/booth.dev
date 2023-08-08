using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Pages.Blog;

public class Index : PageModel
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    public Index(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IReadOnlyCollection<BlogPost> BlogPosts { get; private set; } = ArraySegment<BlogPost>.Empty;

    public string SanitizeContent(string content)
    {
        content = content.Replace("<!--more-->", string.Empty);

        while (content.Contains("\n\n"))
        {
            content = content.Replace("\n\n", "\n");
        }

        return Markdig.Markdown.ToHtml(content.Trim());
    }

    public string TrimContent(string content, out bool trimmed)
    {
        ReadOnlySpan<char> span = content.AsSpan();
        int moreIndex = span.IndexOf("<!--more-->", StringComparison.Ordinal);
        trimmed = moreIndex != -1 || span.Length > 256;
        return moreIndex != -1 ? span[..moreIndex].Trim().ToString() : content.Truncate(256);
    }

    public Author? GetAuthor(BlogPost post)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        return context.Authors.FirstOrDefault(a => a.Id == post.AuthorId);
    }

    public IActionResult OnGet([FromQuery(Name = "p")] int? postId = null)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        if (postId is null)
        {
            BlogPosts = context.BlogPosts.ToArray();
            return Page();
        }

        BlogPost? post = context.BlogPosts.FirstOrDefault(p => p.WordPressId == postId);

        if (post is not null)
        {
            var route = new
            {
                year = post.Published.Year,
                month = post.Published.Month,
                day = post.Published.Day,
                slug = post.Slug
            };
            return Redirect(Url.Page("/Blog/Article", route)!);
        }

        return NotFound();
    }
}
