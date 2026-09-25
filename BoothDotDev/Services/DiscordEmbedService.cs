using BoothDotDev.Data;
using BoothDotDev.Data.Discord;
using BoothDotDev.Data.Models;
using BoothDotDev.Extensions;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service that builds the component embed Discord shows when a link to the site is shared.
/// </summary>
public sealed class DiscordEmbedService
{
    private const int MaxDescriptionLength = 280;
    private const int MaxButtonLabelLength = 80;
    private const int MaxButtons = 3;

    private readonly BlogPostService _blogPostService;
    private readonly LinkService _linkService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DiscordEmbedService" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="linkService">The <see cref="LinkService" />.</param>
    /// <param name="markdownRenderingService">The <see cref="MarkdownRenderingService" />.</param>
    public DiscordEmbedService(
        BlogPostService blogPostService,
        TutorialService tutorialService,
        ProjectService projectService,
        LinkService linkService,
        MarkdownRenderingService markdownRenderingService)
    {
        _blogPostService = blogPostService;
        _tutorialService = tutorialService;
        _projectService = projectService;
        _linkService = linkService;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Builds the JSON of the component embed for a page.
    /// </summary>
    /// <param name="content">The content the page shows, if it shows one.</param>
    /// <param name="siteBaseUrl">The site's own base URL.</param>
    /// <param name="pageUrl">The URL of the page.</param>
    /// <param name="fallbackTitle">The title to use when the page shows no content.</param>
    /// <param name="fallbackDescription">The description to use when the page shows no content.</param>
    /// <returns>The JSON, which is safe to place in a <c>&lt;script&gt;</c> element.</returns>
    public string Build(object? content, Uri siteBaseUrl, Uri pageUrl, string fallbackTitle, string fallbackDescription)
    {
        var page = new Uri(pageUrl.GetLeftPart(UriPartial.Path));
        var embed = content switch
        {
            BlogPost post => ForBlogPost(post, siteBaseUrl, page),
            Note note => ForNote(note, siteBaseUrl, page),
            TutorialArticle article => ForTutorial(article, siteBaseUrl, page),
            DevChallenge challenge => ForChallenge(challenge, siteBaseUrl, page),
            Project project => ForProject(project, siteBaseUrl, page),
            Creation creation => ForCreation(creation, siteBaseUrl, page),
            ProjectDevlog devlog => ForDevlog(devlog, siteBaseUrl, page),
            _ => ForPage(siteBaseUrl, page, fallbackTitle, fallbackDescription)
        };

        return new DiscordComponentEmbed(embed).ToJson();
    }

    private DiscordContainer ForBlogPost(BlogPost post, Uri siteBaseUrl, Uri page)
    {
        var category = _blogPostService.GetCategory(post.CategoryId);
        var hue = post.Color ?? category?.EffectiveColor ?? PaletteHue.Brand;
        var description = _markdownRenderingService.RenderPlainTextExcerpt(post, out _);

        return Compose(hue, post.Title, description, page, HtmlUtility.OgImageUrl(siteBaseUrl, "blog", post.Id),
            Link("Read post", page), Link("More words", new Uri(siteBaseUrl, "/blog")));
    }

    private DiscordContainer ForNote(Note note, Uri siteBaseUrl, Uri page)
    {
        var description = _markdownRenderingService.RenderPlainTextPreview(note.Content);

        return Compose(PaletteHue.Sun, note.Title, description, page, HtmlUtility.OgImageUrl(siteBaseUrl, "note", note.Id),
            Link("Read note", page), Link("More notes", new Uri(siteBaseUrl, "/blog#filter=notes")));
    }

    private DiscordContainer ForTutorial(TutorialArticle article, Uri siteBaseUrl, Uri page)
    {
        var folder = _tutorialService.GetFolder(article.Folder);
        var hue = article.Color ?? (folder.IsSuccess ? folder.Value.EffectiveColor : PaletteHue.Mint);
        var description = _markdownRenderingService.RenderPlainTextExcerpt(article, out _);

        var more = folder.IsSuccess
            ? Link($"More in {folder.Value.Title}", new Uri(siteBaseUrl, $"/learn/{_tutorialService.GetFullSlug(folder.Value)}"))
            : Link("All tutorials", new Uri(siteBaseUrl, "/learn"));

        return Compose(hue, article.Title, description, page, HtmlUtility.OgImageUrl(siteBaseUrl, "tutorial", article.Id),
            Link("Read tutorial", page), more);
    }

    private DiscordContainer ForChallenge(DevChallenge challenge, Uri siteBaseUrl, Uri page)
    {
        var description = _markdownRenderingService.RenderPlainTextExcerpt(challenge, out _);

        return Compose(PaletteHue.Pink, challenge.Title, description, page,
            HtmlUtility.OgImageUrl(siteBaseUrl, "challenge", challenge.Id),
            Link("View challenge", page), Link("All challenges", new Uri(siteBaseUrl, "/challenges")));
    }

    private DiscordContainer ForProject(Project project, Uri siteBaseUrl, Uri page)
    {
        var description = _markdownRenderingService.RenderPlainTextPreview(project.Description);

        return Compose(PaletteHue.Sky, project.Name, description, page, HtmlUtility.OgImageUrl(siteBaseUrl, "project", project.Id),
            [Link("View project", page), .. OwnLinks(project.Id)]);
    }

    private DiscordContainer ForCreation(Creation creation, Uri siteBaseUrl, Uri page)
    {
        var description = string.IsNullOrWhiteSpace(creation.Description)
            ? string.Empty
            : _markdownRenderingService.RenderPlainTextPreview(creation.Description);
        var (type, label) = creation.Kind switch
        {
            CreationKind.Music => ("music", "Listen"),
            CreationKind.ThreeD => ("artwork", "View render"),
            _ => ("artwork", "View drawing")
        };

        return Compose(creation.Kind.ToHue(), creation.Title, description, page,
            HtmlUtility.OgImageUrl(siteBaseUrl, type, creation.Id), [Link(label, page), .. OwnLinks(creation.Id)]);
    }

    private DiscordContainer ForDevlog(ProjectDevlog devlog, Uri siteBaseUrl, Uri page)
    {
        var description = _markdownRenderingService.RenderPlainTextPreview(devlog.Body);
        var project = _projectService.GetProject(devlog.ProjectId);

        var buttons = new List<DiscordButton> { Link("Read devlog", page) };
        if (project.IsSuccess)
        {
            buttons.Add(Link("View project", new Uri(siteBaseUrl, $"/portfolio/{project.Value.Slug}")));
        }

        return Compose(PaletteHue.Sky, devlog.Title, description, page, HtmlUtility.OgImageUrl(siteBaseUrl, "devlog", devlog.Id),
            [.. buttons]);
    }

    private static DiscordContainer ForPage(Uri siteBaseUrl, Uri page, string title, string description)
    {
        var image = new Uri(siteBaseUrl, "/og/site.png").ToString();

        return page.AbsolutePath == "/donate"
            ? Compose(PaletteHue.Brand, title, description, page, image,
                Link("Buy Me a Coffee", new Uri(DonationLinks.BuyMeACoffee)), Link("Ko-fi", new Uri(DonationLinks.KoFi)))
            : Compose(PaletteHue.Brand, title, description, page, image,
                Link("Stuff I made", new Uri(siteBaseUrl, "/portfolio")),
                Link("Words", new Uri(siteBaseUrl, "/blog")),
                Link("Learn", new Uri(siteBaseUrl, "/learn")));
    }

    private static DiscordContainer Compose(
        PaletteHue hue, string title, string? description, Uri page, string imageUrl, params DiscordButton[] buttons)
    {
        var text = $"# **[{EscapeMarkdown(title)}]({page})**";
        var summary = Summarize(description);
        if (summary.Length > 0)
        {
            text += $"\n{summary}";
        }

        return new DiscordContainer(
            AccentOf(hue),
            [
                new DiscordTextDisplay(text),
                new DiscordMediaGallery([new DiscordMediaGalleryItem(new DiscordMedia(imageUrl))]),
                new DiscordSeparator(),
                new DiscordActionRow([.. buttons.Take(MaxButtons)])
            ]);
    }

    private IEnumerable<DiscordButton> OwnLinks(Guid ownerId)
    {
        return _linkService.GetLinks(ownerId)
            .OrderByDescending(link => link.IsPrimary)
            .Select(link => Link(link.Text, new Uri(link.Url)));
    }

    private static DiscordButton Link(string label, Uri url)
    {
        return new DiscordButton(url.ToString(), Truncate(label, MaxButtonLabelLength));
    }

    private static string Summarize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var flat = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return EscapeMarkdown(Truncate(flat, MaxDescriptionLength));
    }

    private static string Truncate(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var cut = text[..(maxLength - 1)];
        var lastSpace = cut.LastIndexOf(' ');
        return $"{(lastSpace > maxLength / 2 ? cut[..lastSpace] : cut).TrimEnd()}…";
    }

    private static string EscapeMarkdown(string text)
    {
        var builder = new System.Text.StringBuilder(text.Length + 8);
        foreach (var ch in text)
        {
            if (ch is '\\' or '*' or '_' or '~' or '`' or '|' or '[' or ']' or '(' or ')' or '<' or '>' or '#' or '@')
            {
                builder.Append('\\');
            }

            builder.Append(ch);
        }

        return builder.ToString();
    }

    // Discord takes an sRGB integer, while the site's colours are oklch tokens in _tokens.css
    private static int AccentOf(PaletteHue hue)
    {
        return hue switch
        {
            PaletteHue.Grape => 0xAB6AEA,
            PaletteHue.Pink => 0xE867C3,
            PaletteHue.Tangerine => 0xFF7041,
            PaletteHue.Sun => 0xF2CF3B,
            PaletteHue.Mint => 0x00CD94,
            PaletteHue.Sky => 0x2EB1EF,
            _ => 0x6161CD
        };
    }
}
