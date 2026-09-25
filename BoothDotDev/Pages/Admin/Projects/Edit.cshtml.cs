using System.ComponentModel.DataAnnotations;
using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using BoothDotDev.Pages.Admin.Portfolio;
using BoothDotDev.Pages.Shared.Partials;
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
public sealed class Edit : PageModel
{
    private readonly LinkService _linkService;
    private readonly MediaService _mediaService;
    private readonly ProjectService _projectService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Edit" /> class.
    /// </summary>
    /// <param name="projectService">The project service.</param>
    /// <param name="mediaService">The <see cref="MediaService" />.</param>
    /// <param name="linkService">The <see cref="LinkService" />.</param>
    public Edit(ProjectService projectService, MediaService mediaService, LinkService linkService)
    {
        _projectService = projectService;
        _mediaService = mediaService;
        _linkService = linkService;
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
    ///     Gets what the media manager shows: the project's files.
    /// </summary>
    /// <value>The media manager, or <see langword="null" /> if a new project is being created.</value>
    public MediaManager? Media { get; private set; }

    /// <summary>
    ///     Gets what the link manager shows: the project's links.
    /// </summary>
    /// <value>The link manager, or <see langword="null" /> if a new project is being created.</value>
    public LinkManager? Links { get; private set; }

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
    /// <remarks>The project's files are untouched. They're managed separately by the media manager.</remarks>
    public IActionResult OnPostSave(Guid? id)
    {
        CreatingNew = id is null;
        ProjectId = id;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var request = BuildSaveRequest();
        var result = id is null ? _projectService.CreateProject(request) : _projectService.UpdateProject(id.Value, request);

        return RedirectOnSuccess(result);
    }

    /// <summary>
    ///     Handles the POST request for deleting the project.
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
    ///     Populates <see cref="Input" /> and <see cref="Media" /> from the given project.
    /// </summary>
    /// <param name="project">The project to populate from.</param>
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
            Status = project.Status,
            Type = project.Type,
            CreatedAt = project.CreatedAt.ToLocalTime()
        };

        Media = new MediaManager
        {
            OwnerId = project.Id,
            Items = _mediaService.GetMedia(MediaOwner.For(project)),
            ReturnUrl = Url.Page("/Admin/Projects/Edit", new { id = project.Id })!,
            Errors = TempData[MediaHandler.MediaErrorsKey] as string
        };

        Links = new LinkManager
        {
            OwnerId = project.Id,
            Items = _linkService.GetLinks(project.Id),
            ReturnUrl = Url.Page("/Admin/Projects/Edit", new { id = project.Id })!,
            Errors = TempData[LinksHandler.LinkErrorsKey] as string
        };
    }

    /// <summary>
    ///     Builds a <see cref="ProjectSaveRequest" /> from <see cref="Input" />.
    /// </summary>
    /// <returns>The save request.</returns>
    private ProjectSaveRequest BuildSaveRequest()
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
            languages,
            Input.Rank,
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
        /// <value>The tagline, or <see langword="null" /> if it has none.</value>
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
        ///     Gets or sets the languages used, comma-separated.
        /// </summary>
        /// <value>The languages.</value>
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Languages { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the rank of the project.
        /// </summary>
        /// <value>The rank.</value>
        public int Rank { get; set; }

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
        /// <value>The creation date and time.</value>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
