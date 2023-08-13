using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data;
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

        using WebContext context = _webContextFactory.CreateDbContext();
        Template? template = context.Templates.Find(templateInline.Name);
        return RenderTemplate(templateInline, template);
    }

    /// <inheritdoc />
    public string RenderTemplate(TemplateInline inline, ITemplate? template)
    {
        if (template is null)
        {
            return $"{{{{{inline.Name}}}}}";
        }

        Span<byte> randomBytes = stackalloc byte[20];
        Random.NextBytes(randomBytes);

        var formatted = new
        {
            inline.ArgumentList,
            inline.ArgumentString,
            inline.Params,
            RandomInt = BinaryPrimitives.ReadInt32LittleEndian(randomBytes[..4]),
            RandomGuid = new Guid(randomBytes[4..]).ToString("N"),
        };

        try
        {
            return _formatter.Format(template.FormatString, formatted);
        }
        catch
        {
            return $"{{{{{inline.Name}|{inline.ArgumentString}}}}}";
        }
    }
}
