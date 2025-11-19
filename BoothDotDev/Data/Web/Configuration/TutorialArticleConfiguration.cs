using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BoothDotDev.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Template" /> entity.
/// </summary>
internal sealed class TutorialArticleConfiguration : IEntityTypeConfiguration<TutorialArticle>
{
    public void Configure(EntityTypeBuilder<TutorialArticle> builder)
    {
        builder.ToTable("article", "tutorials");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Folder).IsRequired();
        builder.Property(e => e.Excerpt).HasMaxLength(512).IsRequired(false);
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated);
        builder.Property(e => e.Slug).IsRequired();
        builder.Property(e => e.Title).IsRequired();
        builder.Property(e => e.PreviewImageUrl).HasConversion<UriToStringConverter>();
        builder.Property(e => e.NextPart);
        builder.Property(e => e.PreviousPart);
        builder.Property(e => e.RedirectFrom).IsRequired(false);
        builder.Property(e => e.Visibility).HasColumnType("visibility").IsRequired();
        builder.Property(e => e.EnableComments).IsRequired();
    }
}
