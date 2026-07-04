namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class BusConfiguration : IEntityTypeConfiguration<Bus>
{
    public void Configure(EntityTypeBuilder<Bus> builder)
    {
        builder.ToTable("Buses");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.PickupLocation).IsRequired().HasMaxLength(300);
        builder.Property(b => b.DestinationLocation).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Price).HasColumnType("decimal(10,2)");

        builder.HasOne(b => b.Edition)
            .WithMany()
            .HasForeignKey(b => b.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.AllowedPassTypes)
            .WithOne(p => p.Bus)
            .HasForeignKey(p => p.BusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(b => b.Reservations)
            .WithOne(r => r.Bus)
            .HasForeignKey(r => r.BusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}