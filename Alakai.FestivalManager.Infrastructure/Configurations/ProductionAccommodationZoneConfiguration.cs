namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionAccommodationZoneConfiguration : IEntityTypeConfiguration<ProductionAccommodationZone>
{
    public void Configure(EntityTypeBuilder<ProductionAccommodationZone> builder)
    {
        builder.ToTable("ProductionAccommodationZones");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(z => z.Accommodations)
            .WithOne(a => a.ProductionAccommodationZone)
            .HasForeignKey(a => a.ProductionAccommodationZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}