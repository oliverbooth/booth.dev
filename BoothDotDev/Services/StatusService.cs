using BoothDotDev.Data;
using BoothDotDev.Data.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace BoothDotDev.Services;

/// <summary>
///     Represents a service responsible for the statuses shown in the thought bubble on the homepage.
/// </summary>
public sealed class StatusService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StatusService" /> class.
    /// </summary>
    /// <param name="dbContextFactory">The database context factory.</param>
    public StatusService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    ///     Gets every status, newest first, whether or not it is in rotation.
    /// </summary>
    /// <returns>The statuses.</returns>
    public IReadOnlyList<Status> GetAll()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return [.. dbContext.Statuses.AsNoTracking().OrderByDescending(s => s.CreatedAt)];
    }

    /// <summary>
    ///     Picks one of the statuses in rotation at random.
    /// </summary>
    /// <returns>The text of the status, or <see langword="null" /> if none is in rotation.</returns>
    public string? GetRandomActive()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.Statuses
            .Where(s => s.IsActive)
            .OrderBy(_ => EF.Functions.Random())
            .Select(s => s.Text)
            .FirstOrDefault();
    }

    /// <summary>
    ///     Adds a status, in rotation.
    /// </summary>
    /// <param name="text">The text of the status.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the status could not be added.</returns>
    public Result Add(string text)
    {
        var textResult = ValidateText(text);
        if (textResult.IsFailed)
        {
            return textResult.ToResult();
        }

        using var dbContext = _dbContextFactory.CreateDbContext();
        dbContext.Statuses.Add(new Status { Text = textResult.Value, IsActive = true });
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Changes the text of a status.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <param name="text">The new text.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the status could not be changed.</returns>
    public Result Update(Guid id, string text)
    {
        var textResult = ValidateText(text);
        if (textResult.IsFailed)
        {
            return textResult.ToResult();
        }

        using var dbContext = _dbContextFactory.CreateDbContext();
        var status = dbContext.Statuses.Find(id);
        if (status is null)
        {
            return NotFound(id);
        }

        status.Text = textResult.Value;
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Puts a status in or out of rotation.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <param name="isActive"><see langword="true" /> to put the status in rotation; otherwise, <see langword="false" />.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the status could not be changed.</returns>
    public Result SetActive(Guid id, bool isActive)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var status = dbContext.Statuses.Find(id);
        if (status is null)
        {
            return NotFound(id);
        }

        status.IsActive = isActive;
        dbContext.SaveChanges();

        return Result.Ok();
    }

    /// <summary>
    ///     Puts a status in rotation and takes every other one out.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the statuses could not be changed.</returns>
    public Result SetOnlyActive(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        // an unknown ID must not fall through to the update below, which would take every status out of rotation
        if (!dbContext.Statuses.Any(s => s.Id == id))
        {
            return NotFound(id);
        }

        dbContext.Statuses.ExecuteUpdate(u => u.SetProperty(s => s.IsActive, s => s.Id == id));

        return Result.Ok();
    }

    /// <summary>
    ///     Removes a status for good.
    /// </summary>
    /// <param name="id">The ID of the status.</param>
    /// <returns>A <see cref="Result" /> indicating success, or why the status could not be removed.</returns>
    public Result Remove(Guid id)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var status = dbContext.Statuses.Find(id);
        if (status is null)
        {
            return NotFound(id);
        }

        dbContext.Statuses.Remove(status);
        dbContext.SaveChanges();

        return Result.Ok();
    }

    private static Result<string> ValidateText(string? text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Fail<string>("A status can't be empty");
        }

        return trimmed.Length > Status.MaxTextLength
            ? Result.Fail<string>($"A status can be at most {Status.MaxTextLength} characters long")
            : trimmed;
    }

    private static Result NotFound(Guid id)
    {
        return Result.Fail($"The status with ID {id} was not found");
    }
}
