namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class AccommodationReservationOccupantConfiguration : IEntityTypeConfiguration<AccommodationReservationOccupant>
{
    public void Configure(EntityTypeBuilder<AccommodationReservationOccupant> builder)
    {
        builder.ToTable("AccommodationReservationOccupants");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Email).IsRequired().HasMaxLength(200);

        builder.HasOne(o => o.Registration)
            .WithMany()
            .HasForeignKey(o => o.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Accommodation)
            .WithMany()
            .HasForeignKey(o => o.AccommodationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}