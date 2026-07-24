namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionAccommodationBuildingConfiguration : IEntityTypeConfiguration<ProductionAccommodationBuilding>
{
    public void Configure(EntityTypeBuilder<ProductionAccommodationBuilding> builder)
    {
        builder.ToTable("ProductionAccommodationBuildings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Type).IsRequired();

        builder.HasOne(b => b.Edition)
            .WithMany()
            .HasForeignKey(b => b.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Zones)
            .WithOne(z => z.ProductionAccommodationBuilding)
            .HasForeignKey(z => z.ProductionAccommodationBuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}