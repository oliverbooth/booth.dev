using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Web.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="Template" /> entity.
/// </summary>
internal sealed class TemplateConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Template");
        builder.HasKey(e => new { e.Name, e.Variant });

        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Variant).HasMaxLength(50).IsRequired();
        builder.Property(e => e.FormatString).IsRequired();
    }
}
