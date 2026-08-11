using System.Buffers.Binary;
using BoothDotDev.Markdown.Template;
using Microsoft.AspNetCore.Mvc.Razor;

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
    private readonly IRazorViewEngine _viewEngine;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    /// <param name="scopeFactory">The <see cref="IServiceScopeFactory" />.</param>
    /// <param name="viewEngine">The <see cref="IRazorViewEngine" />.</param>
    public TemplateService(ILogger<TemplateService> logger,
        IServiceProvider serviceProvider,
        IServiceScopeFactory scopeFactory,
        IRazorViewEngine viewEngine)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        _logger.LogDebug("Registering template override Snippet to CodeSnippetTemplateRenderer");
        AddRendererOverride("Snippet", new CodeSnippetTemplateRenderer(serviceProvider));

        _viewEngine = viewEngine;
    }

    /// <summary>
    ///     Determines whether a template with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <returns>
    ///     <see langword="true" /> if a template with the specified name exists; otherwise, <see langword="false" />.
    /// </returns>
    public bool Exists(string name)
    {
        var partialName = ResolvePartialPath(name, null);
        return PartialExists(partialName);
    }

    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="template">The global template to render.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="template" /> is <see langword="null" />.
    /// </exception>
    public string RenderGlobalTemplate(TemplateInline template)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (template is null)
        {
            _logger.LogWarning("Attempting to render null inline template!");
            throw new ArgumentNullException(nameof(template));
        }

        _logger.LogDebug("Inline name is {Name}", template.Name);
        if (_customTemplateRendererOverrides.TryGetValue(template.Name, out CustomTemplateRenderer? renderer))
        {
            _logger.LogDebug("This matches renderer {Name}", renderer.GetType().Name);
            return renderer.Render(template);
        }

        return Exists(template.Name) ? RenderTemplate(template) : GetDefaultRender(template);
    }

    /// <summary>
    ///     Renders the specified global template with the specified arguments.
    /// </summary>
    /// <param name="template">The global template to render.</param>
    /// <returns>The rendered global template.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="template" /> is <see langword="null" />.
    /// </exception>
    public string RenderTemplate(TemplateInline template)
    {
        var partialName = ResolvePartialPath(template.Name, template.Variant);

        if (!PartialExists(partialName))
        {
            return GetDefaultRender(template);
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var razorPartialRenderer = scope.ServiceProvider.GetRequiredService<RazorPartialRenderer>();
            return razorPartialRenderer
                .RenderToStringAsync(partialName, BuildModel(template))
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template {Name}", template.Name);
            return GetDefaultRender(template);
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
