using System.Text;
using System.Web;
using BoothDotDev.Data;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BoothDotDev.Extensions;

/// <summary>
///     Provides functionality to build a table of contents from markdown content.
/// </summary>
public static class MarkdownTocBuilder
{
    /// <summary>
    ///     Builds a table of contents from the provided markdown source.
    /// </summary>
    /// <param name="markdownSource">The markdown source.</param>
    /// <returns>A list of <see cref="TocItem" /> representing the table of contents.</returns>
    public static List<TocItem> BuildToc(string markdownSource)
    {
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().Build();
        MarkdownDocument document = global::Markdig.Markdown.Parse(markdownSource, pipeline);

        var headings = new List<TocItem>();
        foreach (HeadingBlock block in document.Descendants().OfType<HeadingBlock>())
        {
            int level = block.Level;
            string text = ExtractInlineText(block.Inline);
            headings.Add(new TocItem { Text = text, Level = level });
        }

        MakeUniqueIds(headings);
        return BuildTree(headings);
    }

    private static string ExtractInlineText(ContainerInline? inline)
    {
        if (inline == null)
        {
            return "";
        }

        var sb = new StringBuilder();
        foreach (Inline child in inline)
        {
            switch (child)
            {
                case LiteralInline lit:
                    sb.Append(lit.Content.ToString());
                    break;
                case LinkInline link:
                    // prefer link text if present, else link url
                    if (link.FirstChild != null)
                    {
                        sb.Append(ExtractInlineText(link));
                    }
                    else if (!string.IsNullOrEmpty(link.Url))
                    {
                        sb.Append(link.Url);
                    }

                    break;
                case EmphasisInline emph:
                    sb.Append(ExtractInlineText(emph));
                    break;
                case CodeInline code:
                    sb.Append(code.Content);
                    break;
                default:
                    // generic recursion for other ContainerInline types
                    if (child is ContainerInline c)
                    {
                        sb.Append(ExtractInlineText(c));
                    }

                    break;
            }
        }

        return sb.ToString().Trim();
    }

    private static void MakeUniqueIds(List<TocItem> headings)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (TocItem h in headings)
        {
            string slug = Slugify(h.Text);
            if (string.IsNullOrWhiteSpace(slug))
            {
                slug = "section";
            }

            if (!counts.TryGetValue(slug, out int c))
            {
                c = 0;
            }

            counts[slug] = ++c;

            h.Id = c == 1 ? slug : $"{slug}-{c}";
        }
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "";
        }

        text = text.ToLowerInvariant().Trim();

        var sb = new StringBuilder();
        var lastWasDash = false;

        foreach (char ch in text)
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(ch);
                lastWasDash = false;
            }
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
            {
                if (lastWasDash)
                {
                    continue;
                }

                sb.Append('-');
                lastWasDash = true;
            }
            // ignore other punctuation
        }

        // trim leading/trailing dash
        string result = sb.ToString().Trim('-');
        return result;
    }

    private static List<TocItem> BuildTree(List<TocItem> flat)
    {
        var root = new List<TocItem>();
        var stack = new Stack<TocItem>();

        foreach (TocItem item in flat)
        {
            // if stack empty -> top-level
            while (stack.Count > 0 && item.Level <= stack.Peek().Level)
                stack.Pop();

            if (stack.Count == 0)
            {
                root.Add(item);
            }
            else
            {
                stack.Peek().Children.Add(item);
            }

            stack.Push(item);
        }

        return root;
    }

    /// <summary>
    ///     Renders the table of contents as a markdown list.
    /// </summary>
    /// <param name="root">The root list of <see cref="TocItem" />.</param>
    /// <returns>A markdown string representing the table of contents.</returns>
    public static string RenderTocAsMarkdown(List<TocItem> root)
    {
        var builder = new StringBuilder();
        Render(root, 0);
        return builder.ToString();

        void Render(IEnumerable<TocItem> nodes, int indent)
        {
            foreach (TocItem item in nodes)
            {
                builder.Append(' ', indent * 2); // two spaces per indent
                builder.Append($"- [{EscapeMarkdown(item.Text)}](#{item.Id})\n");
                if (item.Children.Count > 0)
                {
                    Render(item.Children, indent + 1);
                }
            }
        }
    }

    /// <summary>
    ///     Renders the table of contents as an HTML list.
    /// </summary>
    /// <param name="root">The root list of <see cref="TocItem" />.</param>
    /// <param name="request">The current HTTP request (optional).</param>
    /// <returns>An HTML string representing the table of contents.</returns>
    public static string RenderTocAsHtml(List<TocItem> root, HttpRequest? request = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine("<ul>");
        Render(root, 0);
        builder.AppendLine("</ul>");
        return builder.ToString();

        void Render(IEnumerable<TocItem> nodes, int indent)
        {
            foreach (TocItem n in nodes)
            {
                builder.Append(' ', indent * 2); // two spaces per indent
                builder.Append($"<li><a href=\"{request?.Path}#{n.Id}\">{HttpUtility.HtmlEncode(n.Text)}</a>");
                if (n.Children.Count > 0)
                {
                    builder.Append('\n');
                    builder.Append(' ', (indent + 1) * 2);
                    builder.Append("<ul>\n");
                    Render(n.Children, indent + 2);
                    builder.Append(' ', (indent + 1) * 2);
                    builder.Append("</ul>\n");
                    builder.Append(' ', indent * 2);
                }

                builder.Append("</li>\n");
            }
        }
    }

    private static string EscapeMarkdown(string text)
    {
        // minimal escaping for square brackets used in links
        return text.Replace("[", "\\[").Replace("]", "\\]");
    }
}
