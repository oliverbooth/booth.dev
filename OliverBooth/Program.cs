using Asp.Versioning;
using AspNetCore.ReCaptcha;
using Markdig;
using OliverBooth.Data.Blog;
using OliverBooth.Data.Web;
using OliverBooth.Extensions;
using OliverBooth.Markdown.Template;
using OliverBooth.Markdown.Timestamp;
using OliverBooth.Services;
using Serilog;
using X10D.Hosting.DependencyInjection;

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
    .Use(new TemplateExtension(provider.GetRequiredService<ITemplateService>()))
    .UseAdvancedExtensions()
    .UseBootstrap()
    .UseEmojiAndSmiley()
    .UseSmartyPants()
    .Build());

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddDbContextFactory<WebContext>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IContactService, ContactService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<IMastodonService, MastodonService>();
builder.Services.AddSingleton<IReadingListService, ReadingListService>();
builder.Services.AddHostedSingleton<IUserService, UserService>();
builder.Services.AddHostedSingleton<ISessionService, SessionService>();
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
