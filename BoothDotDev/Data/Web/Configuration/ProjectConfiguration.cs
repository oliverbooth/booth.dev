using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Project" /> entity.
/// </summary>
internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("project");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Rank).IsRequired();
        builder.Property(e => e.Slug).IsRequired();
        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.HeroUrl).IsRequired();
        builder.Property(e => e.Description).IsRequired();
        builder.Property(e => e.Details).IsRequired();
        builder.Property(e => e.Status).HasColumnType("project_status").IsRequired();
        builder.Property(e => e.RemoteUrl);
        builder.Property(e => e.RemoteTarget);
        builder.Property(e => e.Tagline);
        builder.Property(e => e.Languages);
    }
}
