using BoothDotDev.Data;
using BoothDotDev.Extensions;
using BoothDotDev.Services;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSingleton<ProjectService>();
builder.Services.AddSingleton<ReadingListService>();
builder.Services.AddSingleton<TemplateService>();
builder.Services.AddSingleton<TutorialService>();
builder.Services.AddSingleton<BlueskyService>();
builder.Services.AddScoped<RazorPartialRenderer>();
builder.Services.Configure<BlueskyOptions>(
    builder.Configuration.GetSection(BlueskyOptions.SectionName));
builder.Services.Configure<WebAuthnOptions>(
    builder.Configuration.GetSection(WebAuthnOptions.SectionName));
builder.Services.AddSingleton<IFido2>(services =>
{
    var webAuthnOptions = services.GetRequiredService<IOptions<WebAuthnOptions>>().Value;
    return new Fido2(new Fido2Configuration
    {
        ServerDomain = webAuthnOptions.RpId,
        ServerName = BoothDotDev.Strings.MyName,
        Origins = new HashSet<string>(webAuthnOptions.Origins)
    });
});
builder.Services.AddMemoryCache();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

WebApplication app = builder.Build();
await ConfigureMigrationsAsync<AppDbContext>(app.Services);

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
app.MapGet("/contact", () => Results.StatusCode(StatusCodes.Status410Gone));
app.MapGet("/contact/blacklist", () => Results.Redirect("/contact", permanent: true));
app.MapGet("/contact/blacklist/formatted/{format}", () => Results.Redirect("/contact", permanent: true));
app.MapGet("/blog/archive", () => Results.Redirect("/blog", permanent: true));
app.MapGet("/blog/posts/{page:int}", () => Results.Redirect("/blog", permanent: true));
app.MapGet("/blog/{year:int}/{month:int}/{day:int}/{slug}", (int year, int month, int day, string slug) =>
{
    var blogPostService = app.Services.GetRequiredService<BlogPostService>();
    var date = new DateOnly(year, month, day);

    var result = blogPostService.GetPost(slug, date);
    return result.IsSuccess
        ? Results.Redirect($"/blog/{slug}", permanent: true)
        : Results.NotFound();
});
app.MapGet("/blog/{year:int}/{month:int}/{day:int}/{slug}/raw", (int year, int month, int day, string slug) =>
{
    var blogPostService = app.Services.GetRequiredService<BlogPostService>();
    var date = new DateOnly(year, month, day);
    var result = blogPostService.GetPost(slug, date);
    return result.IsSuccess
        ? Results.Redirect($"/blog/{slug}/raw", permanent: true)
        : Results.NotFound();
});
app.MapGet("/tutorials", () => Results.Redirect($"/learn", permanent: true));
app.MapGet("/tutorials/{**slug}", (string slug) => Results.Redirect($"/learn/{slug}", permanent: true));
app.MapGet("/tutorial/{**slug}", (string slug) => Results.Redirect($"/learn/{slug}", permanent: true));

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
