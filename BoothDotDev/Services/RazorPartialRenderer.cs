using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service capable of rendering a Razor partial view to a string outside the context of an
///     active MVC/Razor Pages request pipeline.
/// </summary>
public sealed class RazorPartialRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RazorPartialRenderer" /> class.
    /// </summary>
    /// <param name="viewEngine">The <see cref="IRazorViewEngine" /> used to locate partial views.</param>
    /// <param name="tempDataProvider">The <see cref="ITempDataProvider" /> required to construct a view context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" /> used to construct a throwaway HTTP context.</param>
    public RazorPartialRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Renders the specified partial view to a string, using the specified model.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="partialName">The name or path of the partial view to render.</param>
    /// <param name="model">The model to pass to the partial view.</param>
    /// <returns>The rendered HTML of the partial view.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="partialName" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">The specified partial view could not be found.</exception>
    public async Task<string> RenderToStringAsync<TModel>(string partialName, TModel model)
    {
        if (partialName is null)
        {
            throw new ArgumentNullException(nameof(partialName));
        }

        var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        IView view = FindView(actionContext, partialName);

        await using var writer = new StringWriter();

        var viewDataDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary()) { Model = model };

        var tempDataDictionary = new TempDataDictionary(httpContext, _tempDataProvider);

        var viewContext = new ViewContext(
            actionContext,
            view,
            viewDataDictionary,
            tempDataDictionary,
            writer,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);
        return writer.ToString();
    }

    private IView FindView(ActionContext actionContext, string partialName)
    {
        ViewEngineResult result = _viewEngine.GetView(executingFilePath: null, partialName, isMainPage: false);
        if (result.Success)
        {
            return result.View;
        }

        ViewEngineResult fallbackResult = _viewEngine.FindView(actionContext, partialName, isMainPage: false);
        if (fallbackResult.Success)
        {
            return fallbackResult.View;
        }

        IEnumerable<string> searched = result.SearchedLocations.Concat(fallbackResult.SearchedLocations);
        var searchedLocations = string.Join(Environment.NewLine, searched);
        throw new InvalidOperationException($"The partial view '{partialName}' could not be found. " +
                                            $"The following locations were searched:{Environment.NewLine}{searchedLocations}");
    }
}
