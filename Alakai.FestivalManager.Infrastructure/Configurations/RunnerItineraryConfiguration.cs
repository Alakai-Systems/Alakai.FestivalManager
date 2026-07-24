namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class RunnerItineraryConfiguration : IEntityTypeConfiguration<RunnerItinerary>
{
    public void Configure(EntityTypeBuilder<RunnerItinerary> builder)
    {
        builder.ToTable("RunnerItineraries");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.DateTime).IsRequired();
        builder.Property(i => i.Location).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Direction).IsRequired();
        builder.Property(i => i.RunnerName).HasMaxLength(150);
        builder.Property(i => i.Notes).HasMaxLength(1000);

        builder.HasOne(i => i.Edition)
            .WithMany()
            .HasForeignKey(i => i.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}