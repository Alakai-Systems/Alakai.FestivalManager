namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionTripConfiguration : IEntityTypeConfiguration<ProductionTrip>
{
    public void Configure(EntityTypeBuilder<ProductionTrip> builder)
    {
        builder.ToTable("ProductionTrips");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.TripNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.DateTime).IsRequired();
        builder.Property(t => t.TerminalOrStation).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Direction).IsRequired();

        builder.HasOne(t => t.Edition)
            .WithMany()
            .HasForeignKey(t => t.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ProductionPerson)
            .WithMany(p => p.Trips)
            .HasForeignKey(t => t.ProductionPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.RunnerItinerary)
            .WithMany(i => i.Trips)
            .HasForeignKey(t => t.RunnerItineraryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}