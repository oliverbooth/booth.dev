using Cysharp.Text;
using HtmlAgilityPack;

namespace OliverBooth.Pages.Shared.Partials;

/// <summary>
///     Provides methods for displaying pagination tabs.
/// </summary>
public class PageTabsUtility
{
    private string _urlRoot = string.Empty;

    /// <summary>
    ///     Gets or sets the current page number.
    /// </summary>
    /// <value>The current page number.</value>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the page count.
    /// </summary>
    /// <value>The page count.</value>
    public int PageCount { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the URL root.
    /// </summary>
    /// <value>The URL root.</value>
    public string UrlRoot
    {
        get => _urlRoot;
        set => _urlRoot = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    /// <summary>
    ///     Shows the bound chevrons for the specified bounds type.
    /// </summary>
    /// <param name="bounds">The bounds type to display.</param>
    /// <returns>An HTML string containing the elements representing the bound chevrons.</returns>
    public string ShowBounds(BoundsType bounds)
    {
        return bounds switch
        {
            BoundsType.Lower => ShowLowerBound(),
            BoundsType.Upper => ShowUpperBound(PageCount),
            _ => string.Empty
        };
    }

    /// <summary>
    ///     Shows the specified page tab.
    /// </summary>
    /// <param name="tab">The tab to display.</param>
    /// <returns>An HTML string containing the element for the specified page tab.</returns>
    public string ShowTab(int tab)
    {
        var document = new HtmlDocument();
        HtmlNode listItem = document.CreateElement("li");
        HtmlNode pageLink;
        listItem.AddClass("page-item");

        switch (tab)
        {
            case 0:
                // show ... to indicate truncation
                pageLink = document.CreateElement("span");
                pageLink.InnerHtml = "...";
                break;

            case var _ when CurrentPage == tab:
                listItem.AddClass("active");
                listItem.SetAttributeValue("aria-current", "page");

                pageLink = document.CreateElement("span");
                pageLink.InnerHtml = tab.ToString();
                break;

            default:
                pageLink = document.CreateElement("a");
                pageLink.SetAttributeValue("href", GetLinkForPage(tab));
                pageLink.InnerHtml = tab.ToString();
                break;
        }

        pageLink.AddClass("page-link");
        listItem.AppendChild(pageLink);

        document.DocumentNode.AppendChild(listItem);
        return document.DocumentNode.InnerHtml;
    }

    /// <summary>
    ///     Shows the paginated tab window.
    /// </summary>
    /// <returns>An HTML string representing the page tabs.</returns>
    public string ShowTabWindow()
    {
        using Utf16ValueStringBuilder builder = ZString.CreateStringBuilder();

        int windowLowerBound = Math.Max(CurrentPage - 2, 1);
        int windowUpperBound = Math.Min(CurrentPage + 2, PageCount);

        if (windowLowerBound > 2)
        {
            // show lower truncation ...
            builder.AppendLine(ShowTab(0));
        }

        for (int pageIndex = windowLowerBound; pageIndex <= windowUpperBound; pageIndex++)
        {
            if (pageIndex == 1 || pageIndex == PageCount)
            {
                // don't show bounds, these are explicitly written
                continue;
            }

            builder.AppendLine(ShowTab(pageIndex));
        }

        if (windowUpperBound < PageCount - 1)
        {
            // show upper truncation ...
            builder.AppendLine(ShowTab(0));
        }

        return builder.ToString();
    }

    private string GetLinkForPage(int page)
    {
        // page 1 doesn't use /page/n route
        return page == 1 ? _urlRoot : $"{_urlRoot}/page/{page}";
    }

    private string ShowLowerBound()
    {
        if (CurrentPage <= 1)
        {
            return string.Empty;
        }

        var document = new HtmlDocument();
        HtmlNode listItem = document.CreateElement("li");
        listItem.AddClass("page-item");

        HtmlNode pageLink = document.CreateElement("a");
        listItem.AppendChild(pageLink);
        pageLink.AddClass("page-link");
        pageLink.SetAttributeValue("href", UrlRoot);
        pageLink.InnerHtml = "&Lt;";

        document.DocumentNode.AppendChild(listItem);

        listItem = document.CreateElement("li");
        listItem.AddClass("page-item");

        pageLink = document.CreateElement("a");
        listItem.AppendChild(pageLink);
        pageLink.AddClass("page-link");
        pageLink.InnerHtml = "&lt;";
        pageLink.SetAttributeValue("href", GetLinkForPage(CurrentPage - 1));

        document.DocumentNode.AppendChild(listItem);

        return document.DocumentNode.InnerHtml;
    }

    private string ShowUpperBound(int pageCount)
    {
        if (CurrentPage >= pageCount)
        {
            return string.Empty;
        }

        var document = new HtmlDocument();

        HtmlNode pageLink = document.CreateElement("a");
        pageLink.AddClass("page-link");
        pageLink.SetAttributeValue("href", GetLinkForPage(CurrentPage + 1));
        pageLink.InnerHtml = "&gt;";

        HtmlNode listItem = document.CreateElement("li");
        listItem.AddClass("page-item");
        listItem.AppendChild(pageLink);
        document.DocumentNode.AppendChild(listItem);

        pageLink = document.CreateElement("a");
        pageLink.AddClass("page-link");
        pageLink.SetAttributeValue("href", GetLinkForPage(pageCount));
        pageLink.InnerHtml = "&Gt;";

        listItem = document.CreateElement("li");
        listItem.AddClass("page-item");
        listItem.AppendChild(pageLink);
        document.DocumentNode.AppendChild(listItem);

        return document.DocumentNode.InnerHtml;
    }

    public enum BoundsType
    {
        Lower,
        Upper
    }
}
