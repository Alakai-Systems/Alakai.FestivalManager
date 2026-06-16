namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class CompetitionCapacityConfiguration : IEntityTypeConfiguration<CompetitionCapacity>
{
    public void Configure(EntityTypeBuilder<CompetitionCapacity> builder)
    {
        builder.ToTable("CompetitionCapacities");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompetitionId)
            .IsRequired();

        builder.Property(c => c.MixAndMatchLevel);

        builder.Property(c => c.DanceRole)
            .IsRequired();

        builder.Property(c => c.Capacity)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.HasIndex(c => c.CompetitionId);

        builder.HasIndex(c => new { c.CompetitionId, c.MixAndMatchLevel, c.DanceRole })
            .IsUnique();

        builder.HasOne(c => c.Competition)
            .WithMany(c => c.Capacities)
            .HasForeignKey(c => c.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
