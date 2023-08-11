using System.Buffers.Binary;
using Markdig;
using Markdig.Syntax;
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
public sealed class TemplateService
{
    private static readonly Random Random = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly IDbContextFactory<WebContext> _webContextFactory;
    private readonly SmartFormatter _formatter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="webContextFactory">The <see cref="WebContext" /> factory.</param>
    public TemplateService(IServiceProvider serviceProvider, IDbContextFactory<WebContext> webContextFactory)
    {
        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new DefaultSource());
        _formatter.AddExtensions(new ReflectionSource());
        _formatter.AddExtensions(new DateFormatter());
        _formatter.AddExtensions(new MarkdownFormatter(serviceProvider));

        _serviceProvider = serviceProvider;
        _webContextFactory = webContextFactory;
        Current = this;
    }

    public static TemplateService Current { get; private set; } = null!;

    /// <summary>
    ///     Renders the specified template with the specified arguments.
    /// </summary>
    /// <param name="templateInline">The template to render.</param>
    /// <returns>The rendered template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="templateInline" /> is <see langword="null" />.
    /// </exception>
    public string RenderTemplate(TemplateInline templateInline)
    {
        if (templateInline is null) throw new ArgumentNullException(nameof(templateInline));

        using WebContext webContext = _webContextFactory.CreateDbContext();
        ArticleTemplate? template = webContext.ArticleTemplates.Find(templateInline.Name);
        if (template is null)
        {
            return $"{{{{{templateInline.Name}}}}}";
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
            return $"{{{{{templateInline.Name}|{templateInline.ArgumentString}}}}}";
        }
    }
}
