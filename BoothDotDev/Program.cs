using BoothDotDev.Common.Data;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using BoothDotDev.Data;
using BoothDotDev.Data.Blog;
using BoothDotDev.Data.Web;
using BoothDotDev.Extensions;
using BoothDotDev.Extensions.Markdig.Services;
using BoothDotDev.Pages.Components;
using BoothDotDev.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using X10D.Hosting.DependencyInjection;

var workingDir = AppContext.BaseDirectory;

Directory.CreateDirectory(Path.Combine(workingDir, "data"));
Directory.CreateDirectory(Path.Combine(workingDir, "logs"));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(workingDir, "logs", "latest.log"), rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddYamlFile("data/config.yaml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddMarkdownPipeline();
builder.Services.AddDbContextFactory<BlogContext>((services, options) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<BlogContext>>();
    var connectionString = configuration.GetValue<string>("BLOG_CONNECTION_STRING") ?? throw new InvalidOperationException("BLOG_CONNECTION_STRING is not set");

    logger.LogTrace("Using PostgreSQL database provider for BlogContext");
    BlogContextConfig.Configure(options, connectionString);
});

builder.Services.AddDbContextFactory<WebContext>((services, options) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<WebContext>>();
    var connectionString = configuration.GetValue<string>("WEB_CONNECTION_STRING") ?? throw new InvalidOperationException("WEB_CONNECTION_STRING is not set");

    logger.LogTrace("Using PostgreSQL database provider for WebContext");
    WebContextConfig.Configure(options, connectionString);
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICodeSnippetService, CodeSnippetService>();
builder.Services.AddSingleton<IDevChallengeService, DevChallengeService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddHostedSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IBlogUserService, BlogUserService>();
builder.Services.AddSingleton<IProgrammingLanguageService, ProgrammingLanguageService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<ITutorialService, TutorialService>();
builder.Services.AddSingleton<IReadingListService, ReadingListService>();
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddSingleton<BlueskyService>();
builder.Services.Configure<BlueskyOptions>(
    builder.Configuration.GetSection(BlueskyOptions.SectionName));
builder.Services.AddMemoryCache();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

WebApplication app = builder.Build();
await ConfigureMigrationsAsync<BlogContext>(app.Services);
await ConfigureMigrationsAsync<WebContext>(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<SearchComponent>().AddInteractiveServerRenderMode();

app.Run();
return;

async Task ConfigureMigrationsAsync<TContext>(IServiceProvider services) where TContext : DbContext
{
    using var scope = services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();
    await using var context = await factory.CreateDbContextAsync();

    for (var attempt = 1;; attempt++)
    {
        var contextName = typeof(TContext).Name;

        try
        {
            string[] pending = [.. await context.Database.GetPendingMigrationsAsync()];
            if (pending.Length > 0)
            {
                logger.LogInformation("Applying migrations for {DbContext}: {Migrations}", contextName, string.Join(", ", pending));
                await context.Database.MigrateAsync();
            }

            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} for {DbContext} failed. Retrying...", attempt, contextName);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
