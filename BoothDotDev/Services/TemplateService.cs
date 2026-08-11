using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Formatting;
using BoothDotDev.Markdown.Template;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using SmartFormat;
using SmartFormat.Extensions;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service that renders MediaWiki-style templates.
/// </summary>
internal sealed class TemplateService
{
    private static readonly Dictionary<string, string> TemplateNameMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Abbr"] = "Abbr",
        ["ContentWarning"] = "ContentWarning",
        ["GuestPost"] = "GuestPost",
        ["LegacyPost"] = "LegacyPost",
        ["Spoiler"] = "Spoiler"
    };

    private readonly Dictionary<string, CustomTemplateRenderer> _customTemplateRendererOverrides = new();
    private readonly ILogger<TemplateService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IRazorViewEngine _viewEngine;
    private readonly SmartFormatter _formatter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="scopeFactory">The <see cref="IServiceScopeFactory" />.</param>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    /// <param name="viewEngine">The <see cref="IRazorViewEngine" />.</param>
    public TemplateService(ILogger<TemplateService> logger,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRazorViewEngine viewEngine)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new DefaultSource());
        _formatter.AddExtensions(new ReflectionSource());
        _formatter.AddExtensions(new DateFormatter());
        _formatter.AddExtensions(new MarkdownFormatter(serviceProvider));

        _logger.LogDebug("Registering template override Snippet to CodeSnippetTemplateRenderer");
        AddRendererOverride("Snippet", new CodeSnippetTemplateRenderer(serviceProvider));

        _dbContextFactory = dbContextFactory;
        _viewEngine = viewEngine;
    }

    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The global template to render.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    public string RenderGlobalTemplate(TemplateInline templateInline)
    {
        if (templateInline is null)
        {
            _logger.LogWarning("Attempting to render null inline template!");
            throw new ArgumentNullException(nameof(templateInline));
        }

        _logger.LogDebug("Inline name is {Name}", templateInline.Name);
        if (_customTemplateRendererOverrides.TryGetValue(templateInline.Name, out CustomTemplateRenderer? renderer))
        {
            _logger.LogDebug("This matches renderer {Name}", renderer.GetType().Name);
            return renderer.Render(templateInline);
        }

        return TryGetTemplate(templateInline.Name, templateInline.Variant, out Template? template)
            ? RenderTemplate(templateInline, template)
            : GetDefaultRender(templateInline);
    }

    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The global template to render.</param>
    /// <param name="template">The database template object.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    public string RenderTemplate(TemplateInline templateInline, Template? template)
    {
        var partialName = ResolvePartialPath(templateInline.Name, templateInline.Variant);

        if (PartialExists(partialName))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var razorPartialRenderer = scope.ServiceProvider.GetRequiredService<RazorPartialRenderer>();
                return razorPartialRenderer
                    .RenderToStringAsync(partialName, BuildModel(templateInline))
                    .GetAwaiter().GetResult();
            }
            catch
            {
                return GetDefaultRender(templateInline);
            }
        }

        if (template is null)
        {
            return GetDefaultRender(templateInline);
        }

        Span<byte> randomBytes = stackalloc byte[20];
        RandomNumberGenerator.Fill(randomBytes);

        try
        {
            return _formatter.Format(template.FormatString, BuildModel(templateInline));
        }
        catch
        {
            return GetDefaultRender(templateInline);
        }

        static TemplateModel BuildModel(TemplateInline templateInline)
        {
            Span<byte> randomBytes = stackalloc byte[20];
            Random.Shared.NextBytes(randomBytes);

            return new TemplateModel
            {
                ArgumentList = templateInline.ArgumentList,
                ArgumentString = templateInline.ArgumentString,
                Params = templateInline.Params,
                RandomInt = BinaryPrimitives.ReadInt32LittleEndian(randomBytes[..4]),
                RandomGuid = new Guid(randomBytes[4..]).ToString("N"),
                Variant = templateInline.Variant
            };
        }
    }

    /// <summary>
    ///     Attempts to get the template with the specified name.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="template">
    ///     When this method returns, contains the template with the specified name, if the template is found;
    ///     otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the template exists; otherwise, <see langword="false" />.</returns>
    public bool TryGetTemplate(string name, [NotNullWhen(true)] out Template? template)
    {
        return TryGetTemplate(name, string.Empty, out template);
    }

    /// <summary>
    ///     Attempts to get the template with the specified name and variant.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="variant">The variant of the template.</param>
    /// <param name="template">
    ///     When this method returns, contains the template with the specified name and variant, if the template is
    ///     found; otherwise, <see langword="null" />.
    /// </param>
    /// <returns><see langword="true" /> if the template exists; otherwise, <see langword="false" />.</returns>
    public bool TryGetTemplate(string name, string variant, [NotNullWhen(true)] out Template? template)
    {
        var partialName = ResolvePartialPath(name, variant);
        if (PartialExists(partialName))
        {
            // template object is not used!
#pragma warning disable CS8762
            template = null;
            return true;
#pragma warning restore CS8762
        }

        using AppDbContext context = _dbContextFactory.CreateDbContext();
        template = context.Templates.FirstOrDefault(t => t.Name == name && t.Variant == variant);
        return template is not null;
    }

    private void AddRendererOverride(string templateName, CustomTemplateRenderer renderer)
    {
        _logger.LogDebug("Registering template override {Name} to {Renderer}", templateName, renderer.GetType().Name);
        _customTemplateRendererOverrides[templateName] = renderer;
    }

    private static string GetDefaultRender(TemplateInline templateInline)
    {
        return string.IsNullOrWhiteSpace(templateInline.ArgumentString)
            ? $"{{{{{templateInline.Name}}}}}"
            : $"{{{{{templateInline.Name}|{templateInline.ArgumentString}}}}}";
    }

    private bool PartialExists(string partialName)
    {
        return _viewEngine.GetView(executingFilePath: null, partialName, isMainPage: false).Success;
    }

    private string ResolvePartialPath(string name, string? variant)
    {
        if (!TemplateNameMap.TryGetValue(name, out var canonicalName))
        {
            canonicalName = name;
        }

        if (!string.IsNullOrWhiteSpace(variant))
        {
            var variantPath = $"/Views/Templates/_{canonicalName}.{variant}.cshtml";
            if (PartialExists(variantPath))
            {
                return variantPath;
            }
        }

        return $"/Views/Templates/_{canonicalName}.cshtml";
    }
}
