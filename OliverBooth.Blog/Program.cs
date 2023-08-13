using OliverBooth.Blog.Data;
using OliverBooth.Blog.Middleware;
using OliverBooth.Blog.Services;
using OliverBooth.Common;
using OliverBooth.Common.Extensions;
using OliverBooth.Common.Services;
using Serilog;
using X10D.Hosting.DependencyInjection;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.ConfigureOptions<OliverBoothConfigureOptions>();
builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ITemplateService, TemplateService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();

builder.WebHost.AddCertificateFromEnvironment(2846, 5050);

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRssFeed("/feed");
app.MapRazorPages();
app.MapControllers();

app.Run();
