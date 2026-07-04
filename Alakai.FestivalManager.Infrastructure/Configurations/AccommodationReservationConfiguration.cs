namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class AccommodationReservationConfiguration : IEntityTypeConfiguration<AccommodationReservation>
{
    public void Configure(EntityTypeBuilder<AccommodationReservation> builder)
    {
        builder.ToTable("AccommodationReservations");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Edition)
            .WithMany()
            .HasForeignKey(r => r.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AccommodationBuilding)
            .WithMany()
            .HasForeignKey(r => r.AccommodationBuildingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ResponsibleRegistration)
            .WithMany()
            .HasForeignKey(r => r.ResponsibleRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Occupants)
            .WithOne(o => o.AccommodationReservation)
            .HasForeignKey(o => o.AccommodationReservationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}