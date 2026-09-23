using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BoothDotDev.Extensions;

/// <summary>
///     A tag helper that adds an <c>active</c> class to a navigation link if the current page matches one of the specified,
///     comma-separated page match strings.
/// </summary>
[HtmlTargetElement("a", Attributes = "page-match")]
public sealed class ActiveNavLinkTagHelper : TagHelper
{
    /// <summary>
    ///     Gets or sets the comma-separated page match strings to match against the current page route value.
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
        var isActive = PageMatch.Split(',').Any(match =>
            currentPage.Equals(match, StringComparison.OrdinalIgnoreCase)
            || currentPage.StartsWith(match + "/", StringComparison.OrdinalIgnoreCase));

        if (isActive)
        {
            var existing = output.Attributes["class"]?.Value?.ToString();
            output.Attributes.SetAttribute("class", string.IsNullOrEmpty(existing) ? "active" : $"{existing} active");
            output.Attributes.SetAttribute("aria-current", "page");
        }
    }
}
