namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class CompetitionLevelConfiguration : IEntityTypeConfiguration<CompetitionLevel>
{
    public void Configure(EntityTypeBuilder<CompetitionLevel> builder)
    {
        builder.ToTable("CompetitionLevels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.SortOrder)
            .IsRequired();

        builder.Property(l => l.IsActive)
            .IsRequired();

        builder.HasOne(l => l.Competition)
            .WithMany(c => c.Levels)
            .HasForeignKey(l => l.CompetitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.CompetitionId, l.Name })
            .IsUnique();
    }
}

