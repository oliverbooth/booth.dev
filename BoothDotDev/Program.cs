using BoothDotDev.Common.Services;
using BoothDotDev.Data.Blog;
using BoothDotDev.Data.Web;
using BoothDotDev.Extensions;
using BoothDotDev.Extensions.Markdig.Services;
using BoothDotDev.Pages.Components;
using BoothDotDev.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using X10D.Hosting.DependencyInjection;

Directory.CreateDirectory("data");
Directory.CreateDirectory("logs");

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
builder.Services.AddHostedSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IBlogUserService, BlogUserService>();
builder.Services.AddSingleton<IProgrammingLanguageService, ProgrammingLanguageService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();
builder.Services.AddSingleton<ITutorialService, TutorialService>();
builder.Services.AddSingleton<IReadingListService, ReadingListService>();
builder.Services.AddSingleton<ISearchService, SearchService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Admin/Login");
});
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

WebApplication app = builder.Build();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapRazorComponents<SearchComponent>().AddInteractiveServerRenderMode();

app.Run();
