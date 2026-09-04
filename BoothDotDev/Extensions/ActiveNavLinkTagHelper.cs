using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoothDotDev.Extensions;

/// <summary>
///     A tag helper that adds an <c>active</c> class to a navigation link if the current page matches the specified page match
///     string.
/// </summary>
[HtmlTargetElement("a", Attributes = "page-match")]
public sealed class ActiveNavLinkTagHelper : TagHelper
{
    /// <summary>
    ///     Gets or sets the string to match against the current page route value.
    /// </summary>
    /// <value>The page match string.</value>
    [HtmlAttributeName("page-match")]
    public string PageMatch { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the view context.
    /// </summary>
    /// <value>The view context.</value>
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var currentPage = ViewContext.RouteData.Values["page"]?.ToString() ?? "";
        var isActive = currentPage.Equals(PageMatch, StringComparison.OrdinalIgnoreCase)
                       || currentPage.StartsWith(PageMatch + "/", StringComparison.OrdinalIgnoreCase);

        if (isActive)
        {
            var existing = output.Attributes["class"]?.Value?.ToString();
            output.Attributes.SetAttribute("class", string.IsNullOrEmpty(existing) ? "active" : $"{existing} active");
        }
    }
}
