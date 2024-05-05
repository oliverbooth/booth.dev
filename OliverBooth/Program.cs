using AspNetCore.ReCaptcha;
using Markdig;
using OliverBooth.Common.Services;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;
using OliverBooth.Extensions;
using OliverBooth.Extensions.Markdig;
using OliverBooth.Extensions.Markdig.Markdown.Timestamp;
using OliverBooth.Extensions.Markdig.Services;
using OliverBooth.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
#if DEBUG
    .MinimumLevel.Debug()
#endif
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddSingleton(provider => new MarkdownPipelineBuilder()
    .Use<TimestampExtension>()
    .UseTemplates(provider.GetRequiredService<ITemplateService>())

    // we have our own "alert blocks"
    .UseCallouts()

    // advanced extensions. add explicitly to avoid UseAlertBlocks
    .UseAbbreviations()
    .UseAutoIdentifiers()
    .UseCitations()
    .UseCustomContainers()
    .UseDefinitionLists()
    .UseEmphasisExtras()
    .UseFigures()
    .UseFooters()
    .UseFootnotes()
    .UseGridTables()
    .UseMathematics()
    .UseMediaLinks()
    .UsePipeTables()
    .UseListExtras()
    .UseTaskLists()
    .UseDiagrams()
    .UseAutoLinks()
    .UseGenericAttributes() // must be last as it is one parser that is modifying other parsers

    // no more advanced extensions
    .UseBootstrap()
    .UseEmojiAndSmiley()
    .UseSmartyPants()
    .Build());

builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddDbContextFactory<WebContext>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ICodeSnippetService, CodeSnippetService>();
builder.Services.AddSingleton<IContactService, ContactService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IBlogUserService, BlogUserService>();
builder.Services.AddSingleton<IProgrammingLanguageService, ProgrammingLanguageService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<IMastodonService, MastodonService>();
builder.Services.AddSingleton<ITutorialService, TutorialService>();
builder.Services.AddSingleton<IReadingListService, ReadingListService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddReCaptcha(builder.Configuration.GetSection("ReCaptcha"));

if (builder.Environment.IsProduction())
{
    builder.WebHost.AddCertificateFromEnvironment(2845, 5049);
}

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithRedirects("/error/{0}");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
