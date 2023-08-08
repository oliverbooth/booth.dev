using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OliverBooth.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ArticleTemplate" /> entity.
/// </summary>
internal sealed class ArticleTemplateConfiguration : IEntityTypeConfiguration<ArticleTemplate>
{
    public void Configure(EntityTypeBuilder<ArticleTemplate> builder)
    {
        builder.ToTable("ArticleTemplate");
        builder.HasKey(e => e.Name);

        builder.Property(e => e.Name).IsRequired();
        builder.Property(e => e.FormatString).IsRequired();
    }
}
