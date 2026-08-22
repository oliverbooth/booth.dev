using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="TutorialArticle" /> entity.
/// </summary>
internal sealed class TutorialArticleConfiguration : IEntityTypeConfiguration<TutorialArticle>
{
    public void Configure(EntityTypeBuilder<TutorialArticle> builder)
    {
        builder.ToTable("tutorial_article");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.PublishedAt).IsRequired();
        builder.Property(e => e.UpdatedAt).IsRequired(false);
        builder.Property(e => e.Slug).IsRequired();
        builder.Property(e => e.NextPart).IsRequired(false);
        builder.Property(e => e.PreviousPart).IsRequired(false);
        builder.Property(e => e.RedirectFrom).IsRequired(false);
        builder.Property(e => e.EnableComments).IsRequired();
        builder.Property(e => e.CurrentDraftId).IsRequired(false);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasOne(e => e.CurrentDraft).WithMany().HasForeignKey(e => e.CurrentDraftId);

        builder.HasIndex(e => e.TrashedAt);
    }
}
