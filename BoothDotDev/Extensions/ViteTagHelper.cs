using BoothDotDev.Vite;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoothDotDev.Extensions;

/// <summary>
///     Represents a tag helper that resolves <c>src</c> or <c>href</c> attributes on <c>script</c> and <c>link</c> elements
///     through Vite, either via the dev server (development) or the built manifest (production).
/// </summary>
[HtmlTargetElement("script", Attributes = "asp-vite")]
[HtmlTargetElement("link", Attributes = "asp-vite")]
public sealed class ViteTagHelper : TagHelper
{
    /// <summary>
    ///     Gets or sets the source path to resolve through Vite.
    /// </summary>
    /// <value>The source path to resolve through Vite.</value>
    [HtmlAttributeName("asp-vite")]
    public string SourcePath { get; set; } = string.Empty;

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
        output.Attributes.RemoveAll("asp-vite");

        var environment = ViewContext.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var attributeName = output.TagName == "link" ? "href" : "src";

        if (environment.IsDevelopment())
        {
            if (output.TagName == "script")
            {
                output.Attributes.SetAttribute("type", "module");
            }

            output.Attributes.SetAttribute(attributeName, $"{ViteManifest.BaseViteHmrUrl}/{SourcePath}");
            return;
        }

        output.Attributes.SetAttribute(attributeName, ViteManifest.Resolve(SourcePath, environment));
    }
}
