using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

/// <summary>
///     Represents the configuration for the <see cref="ProgrammingLanguage" /> entity.
/// </summary>
internal sealed class ProgrammingLanguageConfiguration : IEntityTypeConfiguration<ProgrammingLanguage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ProgrammingLanguage> builder)
    {
        builder.ToTable("programming_language");
        builder.HasKey(e => e.Key);

        builder.Property(e => e.Key).IsRequired();
        builder.Property(e => e.Name).IsRequired();
    }
}
