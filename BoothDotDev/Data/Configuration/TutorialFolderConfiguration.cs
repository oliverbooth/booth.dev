using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Template" /> entity.
/// </summary>
internal sealed class TutorialFolderConfiguration : IEntityTypeConfiguration<TutorialFolder>
{
    public void Configure(EntityTypeBuilder<TutorialFolder> builder)
    {
        builder.ToTable("tutorial_folder");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Parent);
        builder.Property(e => e.Slug).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PreviewImageUrl).HasConversion<UriToStringConverter>();
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(255);
        builder.Property(e => e.Rank).IsRequired();
    }
}
