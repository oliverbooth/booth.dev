using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BoothDotDev.Data.Blog;

[UsedImplicitly]
internal sealed class BlogContextDesignTimeFactory : IDesignTimeDbContextFactory<BlogContext>
{
    /// <inheritdoc />
    public BlogContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Username=root;Password=localdev;Database=postgres";

        var options = new DbContextOptionsBuilder<BlogContext>();
        BlogContextConfig.Configure(options, connectionString);

        return new BlogContext(options.Options);
    }
}
