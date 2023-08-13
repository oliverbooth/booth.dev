using Markdig;
using OliverBooth.Common;
using OliverBooth.Common.Extensions;
using OliverBooth.Data;
using OliverBooth.Markdown.Template;
using OliverBooth.Markdown.Timestamp;
using OliverBooth.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/latest.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddTomlFile("data/config.toml", true, true);
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.ConfigureOptions<OliverBoothConfigureOptions>();
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

app.MapControllers();
app.MapRazorPages();

app.Run();
