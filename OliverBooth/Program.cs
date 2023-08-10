using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Markdig;
using NLog;
using NLog.Extensions.Logging;
using OliverBooth.Data;
using OliverBooth.Markdown;
using OliverBooth.Markdown.Timestamp;
using OliverBooth.Middleware;
using OliverBooth.Services;
using X10D.Hosting.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);

builder.Logging.ClearProviders();
builder.Logging.AddNLog();
builder.Services.AddHostedSingleton<LoggingService>();
builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<TemplateService>();

builder.Services.AddSingleton(provider => new MarkdownPipelineBuilder()
    .Use<TimestampExtension>()
    .Use(new TemplateExtension(provider.GetRequiredService<TemplateService>()))
    .UseAdvancedExtensions()
    .UseBootstrap()
    .UseEmojiAndSmiley()
    .UseSmartyPants()
    .Build());

builder.Services.AddDbContextFactory<BlogContext>();
builder.Services.AddDbContextFactory<WebContext>();
builder.Services.AddSingleton<BlogService>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.WebHost.UseKestrel(kestrel =>
{
    string certPath = Environment.GetEnvironmentVariable("SSL_CERT_PATH")!;
    if (!File.Exists(certPath))
    {
        kestrel.ListenAnyIP(5049);
        return;
    }

    string? keyPath = Environment.GetEnvironmentVariable("SSL_KEY_PATH");
    if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath)) keyPath = null;

    kestrel.ListenAnyIP(2845, options =>
    {
        X509Certificate2 cert = CreateCertFromPemFile(certPath, keyPath);
        options.UseHttps(cert);
    });
    return;

    static X509Certificate2 CreateCertFromPemFile(string certPath, string? keyPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return X509Certificate2.CreateFromPemFile(certPath, keyPath);

        //workaround for windows issue https://github.com/dotnet/runtime/issues/23749#issuecomment-388231655
        using var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        return new X509Certificate2(cert.Export(X509ContentType.Pkcs12));
    }
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapRssFeed("/blog/feed");

app.Run();

LogManager.Shutdown();
