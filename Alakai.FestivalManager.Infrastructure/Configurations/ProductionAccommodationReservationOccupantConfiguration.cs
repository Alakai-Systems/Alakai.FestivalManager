namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionAccommodationReservationOccupantConfiguration : IEntityTypeConfiguration<ProductionAccommodationReservationOccupant>
{
    public void Configure(EntityTypeBuilder<ProductionAccommodationReservationOccupant> builder)
    {
        builder.ToTable("ProductionAccommodationReservationOccupants");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.IsResponsible).IsRequired();

        builder.HasOne(o => o.ProductionPerson)
            .WithMany(p => p.AccommodationOccupancies)
            .HasForeignKey(o => o.ProductionPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ProductionAccommodation)
            .WithMany()
            .HasForeignKey(o => o.ProductionAccommodationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}