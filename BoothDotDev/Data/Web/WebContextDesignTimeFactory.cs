using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BoothDotDev.Data.Web;

[UsedImplicitly]
internal sealed class WebContextDesignTimeFactory : IDesignTimeDbContextFactory<WebContext>
{
    /// <inheritdoc />
    public WebContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Username=root;Password=localdev;Database=postgres";

        var options = new DbContextOptionsBuilder<WebContext>();
        WebContextConfig.Configure(options, connectionString);

        return new WebContext(options.Options);
    }
}
