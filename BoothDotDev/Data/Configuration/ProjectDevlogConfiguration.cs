using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ProjectDevlog"/> entity.
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
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.EnableComments).IsRequired();
    }
}
