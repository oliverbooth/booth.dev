using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OliverBooth.Common;

/// <summary>
///     Represents the middleware to configure static file options.
/// </summary>
public sealed class OliverBoothConfigureOptions : IPostConfigureOptions<StaticFileOptions>
{
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OliverBoothConfigureOptions" /> class.
    /// </summary>
    /// <param name="environment">The <see cref="IWebHostEnvironment" />.</param>
    public OliverBoothConfigureOptions(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, StaticFileOptions options)
    {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        options.ContentTypeProvider ??= new FileExtensionContentTypeProvider();

        if (options.FileProvider == null && _environment.WebRootFileProvider == null)
        {
            throw new InvalidOperationException("Missing FileProvider.");
        }

        options.FileProvider ??= _environment.WebRootFileProvider;

        var filesProvider = new ManifestEmbeddedFileProvider(GetType().Assembly, "wwwroot");
        options.FileProvider = new CompositeFileProvider(options.FileProvider, filesProvider);
    }
}
