using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
using OliverBooth.Data.Blog;

namespace OliverBooth.Services;

public sealed class BlogService
{
    private IDbContextFactory<BlogContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlogService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The <see cref="IDbContextFactory{TContext}" />.</param>
    public BlogService(IDbContextFactory<BlogContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets a read-only view of all blog posts.
    /// </summary>
    /// <returns>A read-only view of all blog posts.</returns>
    public IReadOnlyCollection<BlogPost> AllPosts
    {
        get
        {
            using BlogContext context = _dbContextFactory.CreateDbContext();
            return context.BlogPosts.OrderByDescending(p => p.Published).ToArray();
        }
    }
}
