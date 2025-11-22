using BoothDotDev.Common.Services;
using BoothDotDev.Data.Blog;
using BoothDotDev.Data.Web;
using BoothDotDev.Extensions;
using BoothDotDev.Extensions.Markdig;
using BoothDotDev.Extensions.Markdig.Markdown.Timestamp;
using BoothDotDev.Extensions.Markdig.Services;
using BoothDotDev.Services;
using Markdig;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddYamlFile("data/config.yaml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddMarkdownPipeline();
builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddDbContextFactory<WebContext>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICodeSnippetService, CodeSnippetService>();
builder.Services.AddSingleton<IDevChallengeService, DevChallengeService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IBlogUserService, BlogUserService>();
builder.Services.AddSingleton<IProgrammingLanguageService, ProgrammingLanguageService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<ITutorialService, TutorialService>();
builder.Services.AddSingleton<IReadingListService, ReadingListService>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

WebApplication app = builder.Build();

app.Use(async (ctx, next) =>
{
    await next();

    if (ctx.Response.HasStarted)
    {
        return;
    }

    string? originalPath = ctx.Request.Path.Value;
    ctx.Items["originalPath"] = originalPath;

    bool matchedErrorPage = false;

    switch (ctx.Response.StatusCode)
    {
        case 400:
            ctx.Request.Path = "/error/401";
            matchedErrorPage = true;
            break;

        case 403:
            ctx.Request.Path = "/error/403";
            matchedErrorPage = true;
            break;

        case 404:
            ctx.Request.Path = "/error/404";
            matchedErrorPage = true;
            break;

        case 410:
            ctx.Request.Path = "/error/410";
            matchedErrorPage = true;
            break;

        case 418:
            ctx.Request.Path = "/error/418";
            matchedErrorPage = true;
            break;

        case 429:
            ctx.Request.Path = "/error/429";
            matchedErrorPage = true;
            break;

        case 500:
            ctx.Request.Path = "/error/500";
            matchedErrorPage = true;
            break;

        case 503:
            ctx.Request.Path = "/error/503";
            matchedErrorPage = true;
            break;

        case 504:
            ctx.Request.Path = "/error/504";
            matchedErrorPage = true;
            break;
    }

    if (matchedErrorPage)
    {
        await next();
    }
});
app.UseStatusCodePagesWithReExecute("/error/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
