namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class AccommodationBuildingConfiguration : IEntityTypeConfiguration<AccommodationBuilding>
{
    public void Configure(EntityTypeBuilder<AccommodationBuilding> builder)
    {
        builder.ToTable("AccommodationBuildings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Type).IsRequired();

        builder.HasOne(b => b.Edition)
            .WithMany()
            .HasForeignKey(b => b.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Zones)
            .WithOne(z => z.AccommodationBuilding)
            .HasForeignKey(z => z.AccommodationBuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.AllowedPassTypes)
            .WithOne(p => p.AccommodationBuilding)
            .HasForeignKey(p => p.AccommodationBuildingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}