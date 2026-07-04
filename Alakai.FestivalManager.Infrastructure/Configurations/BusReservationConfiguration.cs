namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class BusReservationConfiguration : IEntityTypeConfiguration<BusReservation>
{
    public void Configure(EntityTypeBuilder<BusReservation> builder)
    {
        builder.ToTable("BusReservations");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Registration)
            .WithMany()
            .HasForeignKey(r => r.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}