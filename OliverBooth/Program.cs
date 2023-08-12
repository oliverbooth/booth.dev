using Markdig;
using NLog;
using NLog.Extensions.Logging;
using OliverBooth.Common;
using OliverBooth.Common.Extensions;
using OliverBooth.Data;
using OliverBooth.Markdown.Template;
using OliverBooth.Markdown.Timestamp;
using OliverBooth.Services;
using X10D.Hosting.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);

builder.Logging.ClearProviders();
builder.Logging.AddNLog();

builder.Services.ConfigureOptions<OliverBoothConfigureOptions>();
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

builder.Services.AddDbContextFactory<WebContext>();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();
builder.Services.AddCors(options => options.AddPolicy("BlogApi", policy => (builder.Environment.IsDevelopment()
        ? policy.AllowAnyOrigin()
        : policy.WithOrigins("https://oliverbooth.dev"))
    .AllowAnyMethod()
    .AllowAnyHeader()));
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.WebHost.AddCertificateFromEnvironment(2845, 5049);

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
app.UseCors("BlogApi");

app.MapControllers();
app.MapRazorPages();

app.Run();

LogManager.Shutdown();
