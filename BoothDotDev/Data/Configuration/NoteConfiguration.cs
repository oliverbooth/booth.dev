using BoothDotDev.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoothDotDev.Data.Configuration;

internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("note");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).IsRequired();
        builder.Property(e => e.Title).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Content).IsRequired().HasMaxLength(10000);
        builder.Property(e => e.Published).IsRequired();
        builder.Property(e => e.Updated).IsRequired(false);
        builder.Property(e => e.Visibility).IsRequired();
        builder.Property(e => e.FontStyle).IsRequired().HasDefaultValue(FontStyle.Serif);
        builder.Property(e => e.TrashedAt).IsRequired(false);

        builder.HasIndex(e => e.TrashedAt);
    }
}
