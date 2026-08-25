using BoothDotDev.Data;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Markdown.Template;

/// <summary>
///     Represents a custom renderer which overrides the default behaviour of the template engine.
/// </summary>
internal abstract class CustomTemplateRenderer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomTemplateRenderer" /> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    protected CustomTemplateRenderer(IServiceProvider serviceProvider)
    {
        DbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    }

    /// <summary>
    ///     Gets the <see cref="AppDbContext" /> factory that was injected into this instance.
    /// </summary>
    /// <value>An <see cref="IDbContextFactory{TContext}" /> for <see cref="AppDbContext" />.</value>
    protected IDbContextFactory<AppDbContext> DbContextFactory { get; }

    /// <summary>
    ///     Renders the specified template.
    /// </summary>
    /// <param name="template">The template to render.</param>
    /// <returns>The rendered result of the template.</returns>
    public abstract string Render(TemplateInline template);
}
