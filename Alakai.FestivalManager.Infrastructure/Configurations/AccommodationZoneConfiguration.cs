namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class AccommodationZoneConfiguration : IEntityTypeConfiguration<AccommodationZone>
{
    public void Configure(EntityTypeBuilder<AccommodationZone> builder)
    {
        builder.ToTable("AccommodationZones");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(z => z.Accommodations)
            .WithOne(a => a.AccommodationZone)
            .HasForeignKey(a => a.AccommodationZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}