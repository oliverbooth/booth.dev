using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Services;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BoothDotDev.Pages.Admin.Projects;

using Project = Project;

/// <summary>
///     Represents the page model for editing a project in the admin section.
/// </summary>
[Authorize(Policy = "Admin")]
[RequestSizeLimit(CdnUploadPolicy.MaxUploadSizeBytes)]
public sealed class Edit : PageModel
{
    private const string Area = "projects";
    private readonly CdnMediaService _cdnMediaService;

    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    /// <param name="cdnMediaService">The CDN media service.</param>
    public Edit(ProjectService projectService, CdnMediaService cdnMediaService)
    {
        _projectService = projectService;
        _cdnMediaService = cdnMediaService;
    }

    /// <summary>
    ///     Gets or sets the project being edited, if any.
    /// </summary>
    /// <value>The project being edited, or default values if a new project is being created.</value>
    [BindProperty]
    public EditModel Input { get; set; } = new();

    /// <summary>
    ///     Gets a value indicating whether a new project is being created.
    /// </summary>
    /// <value><see langword="true" /> if a new project is being created; otherwise, <see langword="false" />.</value>
    public bool CreatingNew { get; private set; }

    /// <summary>
    ///     Gets the ID of the project being edited.
    /// </summary>
    /// <value>The ID of the project being edited, or <see langword="null" /> if a new project is being created.</value>
    public Guid? ProjectId { get; private set; }

    /// <summary>
    ///     Gets the CDN URL of the project's currently-uploaded hero image, if any.
    /// </summary>
    /// <value>The hero image's CDN URL, or <see langword="null" /> if no hero image has been uploaded yet.</value>
    public string? HeroUrl { get; private set; }

    /// <summary>
    ///     Gets the project's non-trashed devlog entries, newest-published first.
    /// </summary>
    /// <value>The project's devlog entries.</value>
    public IReadOnlyList<ProjectDevlog> Devlogs { get; private set; } = [];

    /// <summary>
    ///     Handles the GET request.
    /// </summary>
    /// <param name="id">The ID of the project to edit. If <see langword="null" />, a new project will be created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnGet(Guid? id)
    {
        if (id is null)
        {
            CreatingNew = true;
            Input = new EditModel
            {
                Status = ProjectStatus.Ongoing, Type = ProjectType.App, CreatedAt = DateTimeOffset.UtcNow.ToLocalTime()
            };
            return Page();
        }

        var projectResult = _projectService.GetProject(id.Value);
        if (projectResult.IsFailed)
        {
            return NotFound();
        }

        var project = projectResult.Value;
        ProjectId = project.Id;
        Devlogs = _projectService.GetDevlogs(project);
        PopulateFromProject(project);

        return Page();
    }

    /// <summary>
    ///     Handles the POST request for saving the project's fields.
    /// </summary>
    /// <param name="id">The ID of the project being edited. If <see langword="null" />, a new project is being created.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    /// <remarks>
    ///     The uploaded hero image (if any) is untouched. It's managed separately by <see cref="OnPostUploadFileAsync" />.
    /// </remarks>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        ProjectId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // The hero image (if any) is untouched here - managed separately by OnPostUploadFileAsync - so its
        // existing filename is carried forward rather than cleared by this metadata-only save.
        string heroUrl;
        if (id is null)
        {
            heroUrl = string.Empty;
        }
        else
        {
            var existingResult = _projectService.GetProject(id.Value);
            if (existingResult.IsFailed)
            {
                return NotFound();
            }

            heroUrl = existingResult.Value.HeroUrl;
        }

        var request = BuildSaveRequest(heroUrl);
        var result = id is null ? _projectService.CreateProject(request) : _projectService.UpdateProject(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for uploading (or replacing) the project's hero image. Every other field is
    ///     left untouched.
    /// </summary>
    /// <param name="id">The ID of the project being edited.</param>
    /// <param name="file">The uploaded image file.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public async Task<IActionResult> OnPostUploadFileAsync(Guid id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("No file was uploaded.");
        }

        var projectResult = _projectService.GetProject(id);
        if (projectResult.IsFailed)
        {
            return NotFound();
        }

        var project = projectResult.Value;

        if (!string.IsNullOrEmpty(project.HeroUrl))
        {
            _cdnMediaService.DeleteFile(id, project.CreatedAt, project.HeroUrl, Area);
        }

        var uploadResult = await _cdnMediaService.UploadAsync(id, project.CreatedAt, file, Area, cancellationToken);
        if (uploadResult.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, uploadResult.Errors.Select(e => e.Message)));
            ProjectId = project.Id;
            Devlogs = _projectService.GetDevlogs(project);
            PopulateFromProject(project);
            return Page();
        }

        var request = new ProjectSaveRequest(
            project.Name,
            project.Slug,
            project.Tagline,
            project.Description,
            project.Details,
            uploadResult.Value.FileName,
            project.Languages,
            project.Rank,
            project.RemoteUrl,
            project.RemoteTarget,
            project.Status,
            project.Type,
            project.CreatedAt);

        _projectService.UpdateProject(id, request);

        return RedirectToPage(new { id });
    }

    /// <summary>
    ///     Handles the POST request for deleting the project. The project must not have any devlog entries,
    ///     trashed or not.
    /// </summary>
    /// <param name="id">The ID of the project to delete.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    public IActionResult OnPostDelete(Guid id)
    {
        var result = _projectService.DeleteProject(id);
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            var projectResult = _projectService.GetProject(id);
            if (projectResult.IsSuccess)
            {
                var project = projectResult.Value;
                ProjectId = project.Id;
                Devlogs = _projectService.GetDevlogs(project);
                PopulateFromProject(project);
            }

            return Page();
        }

        return RedirectToPage("/Admin/Projects/Index");
    }

    /// <summary>
    ///     Populates <see cref="Input" />, <see cref="HeroUrl" />, and related display state from the given project.
    /// </summary>
    private void PopulateFromProject(Project project)
    {
        Input = new EditModel
        {
            Name = project.Name,
            Slug = project.Slug,
            Tagline = project.Tagline,
            Description = project.Description,
            Details = project.Details,
            Languages = string.Join(", ", project.Languages),
            Rank = project.Rank,
            RemoteUrl = project.RemoteUrl,
            RemoteTarget = project.RemoteTarget,
            Status = project.Status,
            Type = project.Type,
            CreatedAt = project.CreatedAt.ToLocalTime()
        };
        HeroUrl = _projectService.GetHeroUrl(project);
    }

    /// <summary>
    ///     Builds a save request from the current state of <see cref="Input" />.
    /// </summary>
    /// <param name="heroUrl">The bare filename of the project's hero image, carried forward from the existing entity.</param>
    private ProjectSaveRequest BuildSaveRequest(string heroUrl)
    {
        var languages = Input.Languages
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new ProjectSaveRequest(
            Input.Name,
            Input.Slug,
            Input.Tagline,
            Input.Description,
            Input.Details,
            heroUrl,
            languages,
            Input.Rank,
            Input.RemoteUrl,
            Input.RemoteTarget,
            Input.Status,
            Input.Type,
            Input.CreatedAt);
    }

    /// <summary>
    ///     Redirects back to this project's edit page on success, or re-renders the form with an error on failure.
    /// </summary>
    /// <param name="result">The result of a save operation.</param>
    /// <returns>An <see cref="IActionResult" /> representing the result of the request.</returns>
    private IActionResult RedirectOnSuccess(Result<Project> result)
    {
        if (result.IsFailed)
        {
            ModelState.AddModelError(string.Empty, string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));
            return Page();
        }

        return RedirectToPage(new { id = result.Value.Id });
    }

    /// <summary>
    ///     Represents the model for editing a project.
    /// </summary>
    public sealed class EditModel
    {
        /// <summary>
        ///     Gets or sets the name of the project.
        /// </summary>
        /// <value>The name.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the slug of the project.
        /// </summary>
        /// <value>The slug.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Slug { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the tagline of the project.
        /// </summary>
        /// <value>The tagline, or <see langword="null" /> if the project has no tagline.</value>
        public string? Tagline { get; set; }

        /// <summary>
        ///     Gets or sets the description of the project.
        /// </summary>
        /// <value>The description.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the details of the project.
        /// </summary>
        /// <value>The details.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Details { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the comma-separated list of languages used for this project.
        /// </summary>
        /// <value>The languages, as a comma-separated string.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Languages { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the rank of the project.
        /// </summary>
        /// <value>The rank.</value>
        public int Rank { get; set; }

        /// <summary>
        ///     Gets or sets the URL of the project.
        /// </summary>
        /// <value>The URL, or <see langword="null" /> if the project has no URL.</value>
        public string? RemoteUrl { get; set; }

        /// <summary>
        ///     Gets or sets the host of the project.
        /// </summary>
        /// <value>The host, or <see langword="null" /> if the project has no remote host.</value>
        public string? RemoteTarget { get; set; }

        /// <summary>
        ///     Gets or sets the status of the project.
        /// </summary>
        /// <value>The status.</value>
        public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;

        /// <summary>
        ///     Gets or sets the type of the project.
        /// </summary>
        /// <value>The type.</value>
        public ProjectType Type { get; set; } = ProjectType.App;

        /// <summary>
        ///     Gets or sets the date and time the project was created.
        /// </summary>
        /// <value>The date and time the project was created.</value>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
