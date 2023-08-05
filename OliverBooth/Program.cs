using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using OliverBooth;
using OliverBooth.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddRazorPages().AddViewLocalization().AddDataAnnotationsLocalization();

builder.Services.AddControllersWithViews();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var supportedCultures = new[]
{
    CultureInfo.GetCultureInfo("en"),
    CultureInfo.GetCultureInfo("fr")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.Configure<LocalizationOptions>(options => options.ResourcesPath = "Resources");

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

app.UseRequestLocalization(options =>
{
    options.ApplyCurrentCultureToResponseHeaders = true;
    options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new RouteCultureProvider(supportedCultures[0]));
});

app.MapControllers();
app.UseCultureRedirect();
app.MapControllerRoute("default", "{culture=en}/{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
