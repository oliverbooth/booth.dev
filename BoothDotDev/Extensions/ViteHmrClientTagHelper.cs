using BoothDotDev.Vite;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoothDotDev.Extensions;

/// <summary>
///     Represents a tag helper that injects the Vite HMR client script into the page when in development mode.
/// </summary>
[HtmlTargetElement("vite-hmr-client")]
public sealed class ViteHmrClientTagHelper : TagHelper
{
    /// <summary>
    ///     Gets or sets the <see cref="ViewContext" /> for the current request.
    /// </summary>
    /// <value>The <see cref="ViewContext" /> for the current request.</value>
    /// <remarks>This property is automatically set by the framework and should not be set manually.</remarks>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var environment = ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        if (!environment.IsDevelopment())
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "script";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("type", "module");
        output.Attributes.SetAttribute("src", $"{ViteManifest.BaseViteHmrUrl}/@vite/client");
    }
}
