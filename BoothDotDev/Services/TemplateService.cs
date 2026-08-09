using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Formatting;
using BoothDotDev.Markdown.Template;
using Microsoft.EntityFrameworkCore;
using SmartFormat;
using SmartFormat.Extensions;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service that renders MediaWiki-style templates.
/// </summary>
internal sealed class TemplateService
{
    private readonly Dictionary<string, CustomTemplateRenderer> _customTemplateRendererOverrides = new();
    private static readonly Random Random = new();
    private readonly ILogger<TemplateService> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly SmartFormatter _formatter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="dbContextFactory">The <see cref="AppDbContext" /> factory.</param>
    public TemplateService(ILogger<TemplateService> logger,
        IServiceProvider serviceProvider,
        IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _logger = logger;

        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new DefaultSource());
        _formatter.AddExtensions(new ReflectionSource());
        _formatter.AddExtensions(new DateFormatter());
        _formatter.AddExtensions(new MarkdownFormatter(serviceProvider));

        _logger.LogDebug("Registering template override Snippet to CodeSnippetTemplateRenderer");
        AddRendererOverride("Snippet", new CodeSnippetTemplateRenderer(serviceProvider));

        _dbContextFactory = dbContextFactory;
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
        if (template is null)
        {
            return GetDefaultRender(templateInline);
        }

        Span<byte> randomBytes = stackalloc byte[20];
        Random.NextBytes(randomBytes);

        var formatted = new
        {
            templateInline.ArgumentList,
            templateInline.ArgumentString,
            templateInline.Params,
            RandomInt = BinaryPrimitives.ReadInt32LittleEndian(randomBytes[..4]),
            RandomGuid = new Guid(randomBytes[4..]).ToString("N"),
        };

        try
        {
            return _formatter.Format(template.FormatString, formatted);
        }
        catch
        {
            return GetDefaultRender(templateInline);
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
}
