using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BoothDotDev.Data;

[UsedImplicitly]
internal sealed class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        const string connectionString = "Host=localhost;Port=5432;Username=root;Password=localdev;Database=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>();
        AppDbContextConfig.Configure(options, connectionString);

        return new AppDbContext(options.Options);
    }
}
