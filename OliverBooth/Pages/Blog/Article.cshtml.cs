﻿using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Pages.Blog;

public class Article : PageModel
{
    private readonly IDbContextFactory<BlogContext> _dbContextFactory;
    private readonly MarkdownPipeline _markdownPipeline;

    public Article(IDbContextFactory<BlogContext> dbContextFactory, MarkdownPipeline markdownPipeline)
    {
        _dbContextFactory = dbContextFactory;
        _markdownPipeline = markdownPipeline;
    }

    public Author Author { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the post is a legacy WordPress post.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the post is a legacy WordPress post; otherwise, <see langword="false" />.
    /// </value>
    public bool IsWordPressLegacyPost => Post?.WordPressId.HasValue ?? false;

    public BlogPost Post { get; private set; } = new();

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
        using BlogContext context = _dbContextFactory.CreateDbContext();
        Post = context.BlogPosts.FirstOrDefault(p => p.Published.Year == year &&
                                                     p.Published.Month == month &&
                                                     p.Published.Day == day &&
                                                     p.Slug == slug)!;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Post is null)
        {
            Response.StatusCode = 404;
            return NotFound();
        }

        Author = context.Authors.FirstOrDefault(a => a.Id == Post.AuthorId)!;
        return Page();
    }
}
