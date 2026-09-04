using BoothDotDev;
using BoothDotDev.Data;
using BoothDotDev.Extensions;
using BoothDotDev.Services;
using Fido2NetLib;
using FluentResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using X10D.Hosting.DependencyInjection;

var workingDir = AppContext.BaseDirectory;

var dataDir = Path.Combine(workingDir, "data");
var logsDir = Path.Combine(workingDir, "logs");
var cdnDir = CdnPaths.GetRoot();
Directory.CreateDirectory(dataDir);
Directory.CreateDirectory(logsDir);
Directory.CreateDirectory(cdnDir);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(logsDir, "latest.log"), rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddYamlFile(Path.Combine(dataDir, "config.yaml"), true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/access-denied";
        options.Cookie.Name = "admin-auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireAuthenticatedUser()));

builder.Services.AddMarkdownPipeline();
builder.Services.AddDbContextFactory<AppDbContext>((services, options) =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var logger = services.GetRequiredService<ILogger<AppDbContext>>();
    var connectionString = configuration.GetValue<string>("DB_CONNECTION_STRING") ??
                           throw new InvalidOperationException("DB_CONNECTION_STRING is not set");

    logger.LogTrace("Using PostgreSQL database provider for AppDbContext");
    AppDbContextConfig.Configure(options, connectionString);
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ActivityService>();
builder.Services.AddHostedSingleton<BlogPostService>();
builder.Services.AddSingleton<CdnMediaService>();
builder.Services.AddSingleton<CdnBrowserService>();
builder.Services.AddSingleton<CommentService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PasskeyService>();
builder.Services.AddSingleton<CodeSnippetService>();
builder.Services.AddSingleton<CreationService>();
builder.Services.AddSingleton<DevChallengeService>();
builder.Services.AddSingleton<MarkdownRenderingService>();
builder.Services.AddSingleton<NoteService>();
builder.Services.AddSingleton<OgImageService>();
builder.Services.AddSingleton<ProjectService>();
builder.Services.AddSingleton<RawContentService>();
builder.Services.AddSingleton<ReadingListService>();
builder.Services.AddSingleton<RssFeedService>();
builder.Services.AddSingleton<TemplateService>();
builder.Services.AddSingleton<TutorialService>();
builder.Services.AddSingleton<BlueskyService>();
builder.Services.AddScoped<RazorPartialRenderer>();
builder.Services.Configure<BlueskyOptions>(
    builder.Configuration.GetSection(BlueskyOptions.SectionName));
builder.Services.Configure<WebAuthnOptions>(
    builder.Configuration.GetSection(WebAuthnOptions.SectionName));
builder.Services.Configure<CdnOptions>(
    builder.Configuration.GetSection(CdnOptions.SectionName));
builder.Services.AddSingleton<IFido2>(services =>
{
    var webAuthnOptions = services.GetRequiredService<IOptions<WebAuthnOptions>>().Value;
    return new Fido2(new Fido2Configuration
    {
        ServerDomain = webAuthnOptions.RpId,
        ServerName = Strings.MyName,
        Origins = new HashSet<string>(webAuthnOptions.Origins)
    });
});
builder.Services.AddMemoryCache();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();
await ConfigureMigrationsAsync<AppDbContext>(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/error/{0}");

// every content listing is subscribable by appending .rss to its URL (/blog.rss, /learn/unity.rss, ...) - a single rule, not one
// route per content type. This has to run as middleware ahead of normal routing rather than as a route itself: /learn/{**slug} is
// already a catch-all Razor Page route, and ASP.NET route templates don't allow a literal suffix after a catch-all segment, so
// "/learn/{**slug}.rss" isn't valid route syntax to register alongside it
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (context.Request.Method != HttpMethods.Get || path is null || !path.EndsWith(".rss", StringComparison.OrdinalIgnoreCase))
    {
        await next(context);
        return;
    }

    await HandleRssFeedAsync(context, path[..^".rss".Length]);
});

// same reasoning as the .rss rule above: appending .md to an article's URL (/blog/foo.md, /learn/unity/awaitable.md, ...)
// shows its raw Markdown source, and /learn/{**slug} being a catch-all means this has to be middleware, not a route
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (context.Request.Method != HttpMethods.Get || path is null || !path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
    {
        await next(context);
        return;
    }

    await HandleRawMarkdownAsync(context, path[..^".md".Length]);
});

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    // `vite`/HMR mode only serves the public dir from its own dev-server port - it copies public/ into wwwroot only on an actual
    // `vite build`. plain (non-asp-vite) static reference, like Prism's autoloader fetching a language file, otherwise 404s in
    // dev until a build has happened at least once
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "..", "public"))
    });
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapGet("/contact", () => Results.StatusCode(StatusCodes.Status410Gone));
app.MapGet("/contact/blacklist", () => Results.Redirect("/contact", true));
app.MapGet("/contact/blacklist/formatted/{format}", () => Results.Redirect("/contact", true));
app.MapGet("/blog/archive", () => Results.Redirect("/blog", true));
app.MapGet("/blog/feed", () => Results.Redirect("/blog.rss", true));
app.MapGet("/blog/page/{page:int}", () => Results.Redirect("/blog", true));
app.MapGet("/blog/posts/{page:int}", () => Results.Redirect("/blog", true));
app.MapGet("/blog/{year:int}/{month:int}/{day:int}/{slug}", (int year, int month, int day, string slug) =>
{
    var blogPostService = app.Services.GetRequiredService<BlogPostService>();
    var date = new DateOnly(year, month, day);

    var result = blogPostService.GetPost(slug, date);
    return result.IsSuccess
        ? Results.Redirect($"/blog/{slug}", true)
        : Results.NotFound();
});
app.MapGet("/blog/{year:int}/{month:int}/{day:int}/{slug}/raw", (int year, int month, int day, string slug) =>
{
    var blogPostService = app.Services.GetRequiredService<BlogPostService>();
    var date = new DateOnly(year, month, day);
    var result = blogPostService.GetPost(slug, date);
    return result.IsSuccess
        ? Results.Redirect($"/blog/{slug}.md", true)
        : Results.NotFound();
});
app.MapGet("/blog/{slug}/raw", (string slug) => Results.Redirect($"/blog/{slug}.md", true));
app.MapGet("/tutorials", () => Results.Redirect("/learn", true));
app.MapGet("/tutorials/{**slug}", (string slug) => Results.Redirect($"/learn/{slug}", true));
app.MapGet("/tutorial/{**slug}", (string slug) => Results.Redirect($"/learn/{slug}", true));

app.Run();
return;

async Task HandleRssFeedAsync(HttpContext context, string path)
{
    var baseUrl = new Uri($"{context.Request.Scheme}://{context.Request.Host}");
    var rssFeedService = context.RequestServices.GetRequiredService<RssFeedService>();
    var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    var xml = segments.Length switch
    {
        1 when segments[0] == "blog" => rssFeedService.BuildBlogFeed(baseUrl),
        1 when segments[0] == "notes" => rssFeedService.BuildNotesFeed(baseUrl),
        1 when segments[0] == "create" => rssFeedService.BuildCreationsFeed(baseUrl),
        1 when segments[0] == "projects" => rssFeedService.BuildProjectsFeed(baseUrl),
        1 when segments[0] == "challenges" => rssFeedService.BuildChallengesFeed(baseUrl),
        1 when segments[0] == "learn" => rssFeedService.BuildTutorialFeed(baseUrl, null),
        > 1 when segments[0] == "learn" => BuildScopedTutorialFeed(context, rssFeedService, baseUrl, segments[1..]),
        _ => null
    };

    if (xml is null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "application/xml";
    await context.Response.WriteAsync(xml);
}

string? BuildScopedTutorialFeed(HttpContext context, RssFeedService rssFeedService, Uri baseUrl, string[] folderSegments)
{
    var tutorialService = context.RequestServices.GetRequiredService<TutorialService>();
    var folderResult = tutorialService.GetFolder(string.Join('/', folderSegments));
    return folderResult.IsSuccess ? rssFeedService.BuildTutorialFeed(baseUrl, folderResult.Value) : null;
}

async Task HandleRawMarkdownAsync(HttpContext context, string path)
{
    var rawContentService = context.RequestServices.GetRequiredService<RawContentService>();
    var segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

    // this middleware runs ahead of UseAuthorization, so the cookie hasn't been authenticated yet - authenticate
    // explicitly rather than trusting context.User, otherwise a signed-in admin would never see private content here
    var authenticateResult = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    var isAuthenticated = authenticateResult.Succeeded;

    var result = segments.Length switch
    {
        2 when segments[0] == "blog" => rawContentService.BuildBlogPostRaw(segments[1], isAuthenticated),
        2 when segments[0] == "note" => rawContentService.BuildNoteRaw(segments[1], isAuthenticated),
        2 when segments[0] == "challenge" => rawContentService.BuildChallengeRaw(segments[1], isAuthenticated),
        4 when segments[0] == "project" && segments[2] == "devlog" =>
            rawContentService.BuildDevlogRaw(segments[1], segments[3], isAuthenticated),
        > 1 when segments[0] == "learn" =>
            rawContentService.BuildTutorialRaw(string.Join('/', segments[1..]), isAuthenticated),
        _ => Result.Fail("No matching route")
    };

    if (result.IsFailed)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    context.Response.ContentType = "text/markdown; charset=utf-8";
    await context.Response.WriteAsync(result.Value);
}

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
                logger.LogInformation("Applying migrations for {Context}: {Migrations}", contextName, string.Join(", ", pending));
                await context.Database.MigrateAsync();
            }

            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            logger.LogWarning(ex, "Migration attempt {Attempt} for {Context} failed. Retrying...", attempt, contextName);
            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }
}
