using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ProjectDevlog" /> entity.
/// </summary>
internal sealed class ProjectDevlogConfiguration : IEntityTypeConfiguration<ProjectDevlog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProjectDevlog> builder)
    {
        builder.ToTable("devlog");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.ProjectId).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Slug).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired();
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.EnableComments).IsRequired();

        // deliberately RESTRICT, not CASCADE: ProjectService.DeleteProject blocks while any devlog (trashed
        // or not) still references the project, so this constraint should never actually fire through normal
        // app usage - it's a DB-level safety net for that same invariant, not a substitute for it
        builder.HasOne<Project>().WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Restrict);
    }
}
