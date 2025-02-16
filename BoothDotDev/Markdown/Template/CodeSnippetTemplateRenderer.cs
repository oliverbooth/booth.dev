using System.Diagnostics;
using System.Text;
using BoothDotDev.Common.Data.Web;
using BoothDotDev.Common.Services;
using BoothDotDev.Extensions.Markdig.Markdown.Template;
using Markdig;

namespace BoothDotDev.Markdown.Template;

/// <summary>
///     Represents a custom template renderer which renders the <c>{{Snippet}}</c> template.
/// </summary>
internal sealed class CodeSnippetTemplateRenderer : CustomTemplateRenderer
{
    private readonly ICodeSnippetService _codeSnippetService;
    private readonly Lazy<MarkdownPipeline> _markdownPipeline;
    private readonly IProgrammingLanguageService _programmingLanguageService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CodeSnippetTemplateRenderer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public CodeSnippetTemplateRenderer(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // lazily evaluate to avoid circular dependency problem causing tremendous stack overflow 
        _markdownPipeline = new Lazy<MarkdownPipeline>(serviceProvider.GetRequiredService<MarkdownPipeline>);
        _codeSnippetService = serviceProvider.GetRequiredService<ICodeSnippetService>();
        _programmingLanguageService = serviceProvider.GetRequiredService<IProgrammingLanguageService>();
    }

    /// <inheritdoc />
    public override string Render(TemplateInline template)
    {
        Debug.Assert(template.Name == "Snippet");
        Trace.Assert(template.Name == "Snippet");

        IReadOnlyList<string> argumentList = template.ArgumentList;

        if (argumentList.Count < 1)
        {
            return DefaultRender(template);
        }

        if (!int.TryParse(argumentList[0], out int snippetId))
        {
            return DefaultRender(template);
        }

        var identifier = Guid.NewGuid();
        var snippets = new List<ICodeSnippet>();

        IReadOnlyList<string> languages = argumentList.Count > 1
            ? argumentList[1].Split(';')
            : _codeSnippetService.GetLanguagesForSnippet(snippetId);

        foreach (string language in languages)
        {
            if (_codeSnippetService.TryGetCodeSnippetForLanguage(snippetId, language, out ICodeSnippet? snippet))
            {
                snippets.Add(snippet);
            }
        }

        if (snippets.Count == 1)
        {
            ICodeSnippet snippet = snippets[0];
            return RenderHtml(snippet);
        }

        var builder = new StringBuilder();
        builder.AppendLine($"""
                            <ul class="nav nav-tabs mb-3" id="snp-{identifier:N}" data-identifier="{identifier:N}" role="tablist"
                            style="margin-bottom: -0.5em !important;">
                            """);

        for (var index = 0; index < languages.Count; index++)
        {
            var language = languages[index];
            string classList = "";
            if (index == 0)
            {
                classList = " active";
            }

            builder.AppendLine("""<li class="nav-item" role="presentation">""");
            builder.AppendLine($"""
                                    <a
                                      data-tab-init
                                      class="nav-link{classList}"
                                      id="snp-{snippetId}-{identifier:N}-{language}-l"
                                      href="#"
                                      role="tab"
                                      data-tabs="snp-{snippetId}-{identifier:N}"
                                      aria-controls="snp-{snippetId}-{identifier:N}-{language}"
                                      aria-selected="true"
                                      >{_programmingLanguageService.GetLanguageName(language)}</a
                                    >
                                """);
            builder.AppendLine("</li>");
        }

        builder.AppendLine("</ul>");

        builder.AppendLine($"""<div class="tab-content" id="snp-{snippetId}-{identifier:N}">""");

        for (var index = 0; index < snippets.Count; index++)
        {
            string classList = "";
            if (index == 0)
            {
                classList = " show active";
            }

            var snippet = snippets[index];
            string html = RenderHtml(snippet);
            builder.AppendLine($"""
                                <div class="tab-pane fade{classList}" id="snp-{snippetId}-{identifier:N}-{snippet.Language}" data-identifier="{identifier:N}" role="tabpanel"
                                aria-labelledby="snp-{snippetId}-{identifier:N}-{snippet.Language}">
                                """);
            builder.AppendLine(html);
            builder.AppendLine("</div>");
        }

        builder.AppendLine("</div>");

        return builder.ToString();
    }

    private string RenderHtml(ICodeSnippet snippet)
    {
        return Markdig.Markdown.ToHtml($"```{snippet.Language}\n{snippet.Content}\n```", _markdownPipeline.Value);
    }

    private static string DefaultRender(TemplateInline template)
    {
        return template.ArgumentList.Count == 0
            ? $"{{{{{template.Name}}}}}"
            : $"{{{{{template.Name}|{string.Join('|', template.ArgumentList)}}}}}";
    }
}
