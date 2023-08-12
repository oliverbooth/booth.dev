using OliverBooth.Blog.Data;
using OliverBooth.Blog.Middleware;
using OliverBooth.Blog.Services;
using OliverBooth.Common;
using OliverBooth.Common.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);

builder.Services.ConfigureOptions<OliverBoothConfigureOptions>();
builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddSingleton<IBlogPostService, BlogPostService>();
builder.Services.AddSingleton<IUserService, UserService>();
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
