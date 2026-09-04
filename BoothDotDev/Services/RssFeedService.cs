using System.Xml.Serialization;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Data.Models.Rss;
using DEDrake;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service for building the site's RSS feeds.
/// </summary>
public sealed class RssFeedService
{
    private readonly BlogPostService _blogPostService;
    private readonly CreationService _creationService;
    private readonly DevChallengeService _devChallengeService;
    private readonly MarkdownRenderingService _markdownRenderingService;
    private readonly NoteService _noteService;
    private readonly ProjectService _projectService;
    private readonly TutorialService _tutorialService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RssFeedService" /> class.
    /// </summary>
    /// <param name="blogPostService">The <see cref="BlogPostService" />.</param>
    /// <param name="noteService">The <see cref="NoteService" />.</param>
    /// <param name="tutorialService">The <see cref="TutorialService" />.</param>
    /// <param name="creationService">The <see cref="CreationService" />.</param>
    /// <param name="projectService">The <see cref="ProjectService" />.</param>
    /// <param name="devChallengeService">The <see cref="DevChallengeService" />.</param>
    /// <param name="markdownRenderingService">The <see cref="MarkdownRenderingService" />.</param>
    public RssFeedService(
        BlogPostService blogPostService,
        NoteService noteService,
        TutorialService tutorialService,
        CreationService creationService,
        ProjectService projectService,
        DevChallengeService devChallengeService,
        MarkdownRenderingService markdownRenderingService)
    {
        _blogPostService = blogPostService;
        _noteService = noteService;
        _tutorialService = tutorialService;
        _creationService = creationService;
        _projectService = projectService;
        _devChallengeService = devChallengeService;
        _markdownRenderingService = markdownRenderingService;
    }

    /// <summary>
    ///     Builds the RSS feed for the blog.
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildBlogFeed(Uri baseUrl)
    {
        var feedUrl = new Uri(baseUrl, "/blog");
        var blogItems = new List<BlogItem>();

        foreach (var post in _blogPostService.GetAllBlogPosts())
        {
            var url = new Uri(baseUrl, $"/blog/{post.Slug}").ToString();
            var excerpt = _markdownRenderingService.RenderExcerpt(post, out _);
            var description = $"{excerpt}<p><a href=\"{url}\">Read more...</a></p>";

            blogItems.Add(new BlogItem
            {
                Title = post.Title,
                Link = url,
                Comments = $"{url}#comments",
                Creator = post.Author.DisplayName,
                PubDate = post.PublishedAt.ToString("R"),
                Guid = post.WordPressId.HasValue ? $"{feedUrl}?p={post.WordPressId.Value}" : $"{feedUrl}?pid={post.Id}",
                Description = description
            });
        }

        var rss = new BlogRoot
        {
            Channel = new BlogChannel
            {
                AtomLink = new AtomLink { Href = new Uri(baseUrl, "/blog.rss").ToString() },
                Description = $"{feedUrl}/",
                LastBuildDate = DateTimeOffset.UtcNow.ToString("R"),
                Link = $"{feedUrl}/",
                Title = Strings.MyName,
                Generator = $"{feedUrl}/",
                Items = blogItems
            }
        };

        return Serialize(rss);
    }

    /// <summary>
    ///     Builds the RSS feed for notes.
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildNotesFeed(Uri baseUrl)
    {
        var items = new List<RssItem>();

        foreach (var note in _noteService.GetAllNotes())
        {
            var url = new Uri(baseUrl, $"/note/{(ShortGuid)note.Id}").ToString();
            items.Add(new RssItem
            {
                Title = note.Title,
                Link = url,
                PubDate = note.PublishedAt.ToString("R"),
                Guid = url,
                Description = _markdownRenderingService.RenderHtmlPreview(note.Content)
            });
        }

        return BuildGenericFeed(baseUrl, "/notes", $"Notes by {Strings.MyName}", items);
    }

    /// <summary>
    ///     Builds the RSS feed for tutorials.
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <param name="scope">
    ///     The folder to scope the feed to (that folder and every descendant folder's articles), or
    ///     <see langword="null" /> for every tutorial site-wide.
    /// </param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildTutorialFeed(Uri baseUrl, TutorialFolder? scope)
    {
        var articles = scope is null
            ? _tutorialService.GetAllArticles(Visibility.Published)
            : _tutorialService.GetArticlesInSubtree(scope, Visibility.Published);

        var items = new List<RssItem>();
        foreach (var article in articles)
        {
            var url = new Uri(baseUrl, $"/learn/{_tutorialService.GetFullSlug(article)}").ToString();
            var excerpt = _markdownRenderingService.RenderExcerpt(article, out _);

            items.Add(new RssItem
            {
                Title = article.Title,
                Link = url,
                PubDate = article.PublishedAt.ToString("R"),
                Guid = url,
                Description = excerpt
            });
        }

        var path = scope is null ? "/learn" : $"/learn/{_tutorialService.GetFullSlug(scope)}";
        var title = scope is null ? $"Tutorials by {Strings.MyName}" : scope.Title;
        return BuildGenericFeed(baseUrl, path, title, items);
    }

    /// <summary>
    ///     Builds the RSS feed for creations (artwork and music).
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildCreationsFeed(Uri baseUrl)
    {
        // Artwork/music items have no page of their own - every item links to the shared /create listing, but the
        // guid stays unique per item so subscribers can still tell entries apart.
        var pageUrl = new Uri(baseUrl, "/create").ToString();
        var items = new List<RssItem>();

        foreach (var item in _creationService.GetArtworkItems().Cast<CreativeItem>()
                     .Concat(_creationService.GetMusicItems()))
        {
            items.Add(new RssItem
            {
                Title = item.Title,
                Link = pageUrl,
                PubDate = item.PublishedAt.ToString("R"),
                Guid = new RssItemGuid { Value = $"{pageUrl}#{item.Id:N}", IsPermaLink = false },
                Description = string.IsNullOrWhiteSpace(item.Description)
                    ? string.Empty
                    : _markdownRenderingService.RenderHtmlPreview(item.Description)
            });
        }

        return BuildGenericFeed(baseUrl, "/create", $"Creations by {Strings.MyName}", items);
    }

    /// <summary>
    ///     Builds the RSS feed for projects.
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildProjectsFeed(Uri baseUrl)
    {
        var items = new List<RssItem>();

        var projects = _projectService.GetProjects()
            .Concat(_projectService.GetProjects(ProjectStatus.Past))
            .Concat(_projectService.GetProjects(ProjectStatus.Retired))
            .Concat(_projectService.GetProjects(ProjectStatus.Hiatus));

        foreach (var project in projects)
        {
            var url = new Uri(baseUrl, $"/project/{project.Slug}").ToString();
            items.Add(new RssItem
            {
                Title = project.Name,
                Link = url,
                PubDate = project.CreatedAt.ToString("R"),
                Guid = url,
                Description = _markdownRenderingService.RenderHtmlPreview(project.Description)
            });
        }

        return BuildGenericFeed(baseUrl, "/projects", $"Projects by {Strings.MyName}", items);
    }

    /// <summary>
    ///     Builds the RSS feed for dev challenges.
    /// </summary>
    /// <param name="baseUrl">The site's own base URL, used to build absolute links.</param>
    /// <returns>The serialized RSS feed.</returns>
    public string BuildChallengesFeed(Uri baseUrl)
    {
        var items = new List<RssItem>();

        foreach (var challenge in _devChallengeService.GetAllChallenges())
        {
            var url = new Uri(baseUrl, $"/challenge/{challenge.Id}").ToString();
            var excerpt = _markdownRenderingService.RenderExcerpt(challenge, out _);

            items.Add(new RssItem
            {
                Title = challenge.Title,
                Link = url,
                PubDate = challenge.PublishedAt.ToString("R"),
                Guid = url,
                Description = excerpt
            });
        }

        return BuildGenericFeed(baseUrl, "/challenges", $"Dev Challenges by {Strings.MyName}", items);
    }

    private static string BuildGenericFeed(Uri baseUrl, string path, string title, IReadOnlyList<RssItem> items)
    {
        var pageUrl = new Uri(baseUrl, path).ToString();
        var rss = new RssRoot
        {
            Channel = new RssChannel
            {
                AtomLink = new AtomLink { Href = new Uri(baseUrl, $"{path}.rss").ToString() },
                Description = $"{pageUrl}/",
                LastBuildDate = DateTimeOffset.UtcNow.ToString("R"),
                Link = $"{pageUrl}/",
                Title = title,
                Generator = $"{pageUrl}/",
                Items = [.. items]
            }
        };

        return Serialize(rss);
    }

    private static string Serialize<TRoot>(TRoot root) where TRoot : class
    {
        var serializer = new XmlSerializer(typeof(TRoot));
        var xmlNamespaces = new XmlSerializerNamespaces();
        xmlNamespaces.Add("content", "http://purl.org/rss/1.0/modules/content/");
        xmlNamespaces.Add("wfw", "http://wellformedweb.org/CommentAPI/");
        xmlNamespaces.Add("dc", "http://purl.org/dc/elements/1.1/");
        xmlNamespaces.Add("atom", "http://www.w3.org/2005/Atom");
        xmlNamespaces.Add("sy", "http://purl.org/rss/1.0/modules/syndication/");
        xmlNamespaces.Add("slash", "http://purl.org/rss/1.0/modules/slash/");

        using var writer = new StringWriter();
        serializer.Serialize(writer, root, xmlNamespaces);
        return writer.ToString();
    }
}
