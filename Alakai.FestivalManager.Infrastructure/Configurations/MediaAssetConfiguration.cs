using Alakai.FestivalManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.ToTable("MediaAssets");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasOne(m => m.Festival)
            .WithMany()
            .HasForeignKey(m => m.FestivalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}