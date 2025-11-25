using System.Text.RegularExpressions;
using BoothDotDev.Common.Data.Blog;
using BoothDotDev.Common.Services;
using DEDrake;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BoothDotDev.Pages.Components.Admin;

/// <summary>
///     Represents a Blazor component for editing a blog post.
/// </summary>
public partial class EditPostComponent : ComponentBase
{
    private static readonly Regex InvalidSlugRegex = GetInvalidSlugRegex();
    private readonly IBlogPostService _blogPostService;
    private readonly IJSRuntime _jsRuntime;
    private EditContext _editContext = null!;
    private bool _showPassword;
    private bool _showShortId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditPostComponent" /> class.
    /// </summary>
    /// <param name="blogPostService">The blog post service.</param>
    /// <param name="jsRuntime">The JavaScript runtime.</param>
    public EditPostComponent(IBlogPostService blogPostService, IJSRuntime jsRuntime)
    {
        _blogPostService = blogPostService;
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    ///     Gets or sets the blog post ID.
    /// </summary>
    /// <value>The blog post ID.</value>
    [Parameter]
    public Guid BlogPostId { get; set; }

    /// <summary>
    ///     Gets the blog post being edited.
    /// </summary>
    /// <value>The blog post being edited.</value>
    private BlogPostEditModel BlogPost { get; set; } = null!;

    /// <summary>
    ///     Gets the displayed ID, either short or full based on user preference.
    /// </summary>
    /// <value>The displayed ID.</value>
    private string DisplayedId => _showShortId ? ((ShortGuid)BlogPostId).ToString() : BlogPostId.ToString();

    /// <summary>
    ///     Gets or sets a value indicating whether the blog post has unsaved changes.
    /// </summary>
    /// <value><see langword="true" /> if the blog post has unsaved changes; otherwise, <see langword="false" />.</value>
    private bool IsDirty { get; set; }

    /// <summary>
    ///     Gets the input type for the password field.
    /// </summary>
    /// <value>The input type for the password field.</value>
    private string PasswordInputType => _showPassword ? "text" : "password";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        if (!_blogPostService.TryGetPost(BlogPostId, out IBlogPost? post))
        {
            return;
        }

        BlogPost = new BlogPostEditModel(post);
        _editContext = new EditContext(BlogPost);
        _editContext.OnFieldChanged += (_, _) => IsDirty = true;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsDirty = false;
        }

        await _jsRuntime.InvokeVoidAsync("lucide.createIcons");
    }

    private void RegenerateSlug()
    {
        BlogPost.Slug = BlogPost.Title.Kebaberize();
        SanitizeSlug();
        StateHasChanged();
    }

    private void SanitizeSlug()
    {
        string currentSlug = BlogPost.Slug;

        if (string.IsNullOrWhiteSpace(currentSlug))
        {
            return;
        }

        string slug = InvalidSlugRegex.Replace(currentSlug.Kebaberize(), string.Empty);
        if (currentSlug != slug)
        {
            BlogPost.Slug = slug;
        }
    }

    private void Save()
    {
        try
        {
            _blogPostService.UpdateBlogPost(BlogPost);
            _editContext.MarkAsUnmodified();
            IsDirty = false;
            _jsRuntime.InvokeVoidAsync("alert", "Blog post saved successfully.");
        }
        catch (Exception ex)
        {
            _jsRuntime.InvokeVoidAsync("alert", $"Error saving blog post: {ex.Message}");
        }
    }

    private void ToggleIdDisplayMode()
    {
        _showShortId = !_showShortId;
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    [GeneratedRegex(@"[^a-z0-9-]", RegexOptions.Compiled)]
    private static partial Regex GetInvalidSlugRegex();
}
