using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Web;
using OliverBooth.Formatting;
using OliverBooth.Markdown.Template;
using SmartFormat;
using SmartFormat.Extensions;

namespace OliverBooth.Services;

/// <summary>
///     Represents a service that renders MediaWiki-style templates.
/// </summary>
internal sealed class TemplateService : ITemplateService
{
    private static readonly Random Random = new();
    private readonly IDbContextFactory<WebContext> _webContextFactory;
    private readonly SmartFormatter _formatter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="webContextFactory">The <see cref="WebContext" /> factory.</param>
    public TemplateService(IServiceProvider serviceProvider,
        IDbContextFactory<WebContext> webContextFactory)
    {
        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new DefaultSource());
        _formatter.AddExtensions(new ReflectionSource());
        _formatter.AddExtensions(new DateFormatter());
        _formatter.AddExtensions(new MarkdownFormatter(serviceProvider));

        _webContextFactory = webContextFactory;
    }

    /// <inheritdoc />
    public string RenderGlobalTemplate(TemplateInline templateInline)
    {
        if (templateInline is null) throw new ArgumentNullException(nameof(templateInline));

        return TryGetTemplate(templateInline.Name, templateInline.Variant, out ITemplate? template)
            ? RenderTemplate(templateInline, template)
            : GetDefaultRender(templateInline);
    }

    /// <inheritdoc />
    public string RenderTemplate(TemplateInline templateInline, ITemplate? template)
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

    /// <inheritdoc />
    public bool TryGetTemplate(string name, [NotNullWhen(true)] out ITemplate? template)
    {
        return TryGetTemplate(name, string.Empty, out template);
    }

    /// <inheritdoc />
    public bool TryGetTemplate(string name, string variant, [NotNullWhen(true)] out ITemplate? template)
    {
        using WebContext context = _webContextFactory.CreateDbContext();
        template = context.Templates.FirstOrDefault(t => t.Name == name && t.Variant == variant);
        return template is not null;
    }

    private static string GetDefaultRender(TemplateInline templateInline)
    {
        return string.IsNullOrWhiteSpace(templateInline.ArgumentString)
            ? $"{{{{{templateInline.Name}}}}}"
            : $"{{{{{templateInline.Name}|{templateInline.ArgumentString}}}}}";
    }
}
