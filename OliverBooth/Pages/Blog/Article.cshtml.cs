using Humanizer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Pages.Blog;

public class Article : PageModel
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;

    public Article(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public Author? Author { get; private set; }

    public BlogPost? Post { get; private set; }

    public string SanitizeContent(string content)
    {
        content = content.Replace("<more>", string.Empty);

        while (content.Contains("\n\n"))
        {
            content = content.Replace("\n\n", "\n");
        }

        return Markdig.Markdown.ToHtml(content.Trim());
    }

    public void OnGet(int year, int month, string slug)
    {
        using BlogContext context = _dbContextFactory.CreateDbContext();
        Post = context.BlogPosts.FirstOrDefault(p => p.Published.Year == year &&
                                                     p.Published.Month == month &&
                                                     p.Slug == slug);

        if (Post is null)
        {
            Response.StatusCode = 404;
        }
        else
        {
            Author = context.Authors.FirstOrDefault(a => a.Id == Post.AuthorId);
        }
    }
}
