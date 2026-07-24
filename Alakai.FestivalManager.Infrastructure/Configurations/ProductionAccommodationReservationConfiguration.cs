namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionAccommodationReservationConfiguration : IEntityTypeConfiguration<ProductionAccommodationReservation>
{
    public void Configure(EntityTypeBuilder<ProductionAccommodationReservation> builder)
    {
        builder.ToTable("ProductionAccommodationReservations");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Edition)
            .WithMany()
            .HasForeignKey(r => r.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ProductionAccommodationBuilding)
            .WithMany()
            .HasForeignKey(r => r.ProductionAccommodationBuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResponsibleProductionPerson)
            .WithMany()
            .HasForeignKey(r => r.ResponsibleProductionPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Occupants)
            .WithOne(o => o.ProductionAccommodationReservation)
            .HasForeignKey(o => o.ProductionAccommodationReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}