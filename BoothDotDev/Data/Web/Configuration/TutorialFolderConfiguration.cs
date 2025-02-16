using BoothDotDev.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Template" /> entity.
/// </summary>
internal sealed class TutorialFolderConfiguration : IEntityTypeConfiguration<TutorialFolder>
{
    public void Configure(EntityTypeBuilder<TutorialFolder> builder)
    {
        builder.ToTable("TutorialFolder");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Parent);
        builder.Property(e => e.Slug).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PreviewImageUrl).HasConversion<UriToStringConverter>();
        builder.Property(e => e.Visibility).HasConversion<EnumToStringConverter<Visibility>>();
        builder.Property(e => e.Description).HasMaxLength(255);
    }
}
