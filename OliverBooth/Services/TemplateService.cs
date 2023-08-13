using System.Buffers.Binary;
using Microsoft.EntityFrameworkCore;
using OliverBooth.Data.Blog;
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
    private readonly IDbContextFactory<BlogContext> _webContextFactory;
    private readonly SmartFormatter _formatter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="webContextFactory">The <see cref="BlogContext" /> factory.</param>
    public TemplateService(IServiceProvider serviceProvider, IDbContextFactory<BlogContext> webContextFactory)
    {
        _formatter = Smart.CreateDefaultSmartFormat();
        _formatter.AddExtensions(new DefaultSource());
        _formatter.AddExtensions(new ReflectionSource());
        _formatter.AddExtensions(new DateFormatter());
        _formatter.AddExtensions(new MarkdownFormatter(serviceProvider));

        _webContextFactory = webContextFactory;
    }

    /// <inheritdoc />
    public string RenderTemplate(TemplateInline templateInline)
    {
        if (templateInline is null) throw new ArgumentNullException(nameof(templateInline));

        using BlogContext webContext = _webContextFactory.CreateDbContext();
        Template? template = webContext.Templates.Find(templateInline.Name);
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
