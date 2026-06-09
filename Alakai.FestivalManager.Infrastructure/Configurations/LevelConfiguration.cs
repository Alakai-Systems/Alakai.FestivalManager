namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> builder)
    {
        builder.ToTable("Levels");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.Description)
            .HasMaxLength(1000);

        builder.Property(l => l.EarlyBirdPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(l => l.GroupPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(l => l.RegularPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(l => l.SortOrder)
            .IsRequired();

        builder.Property(l => l.IsActive)
            .IsRequired();

        builder.HasOne(l => l.PassType)
            .WithMany(p => p.Levels)
            .HasForeignKey(l => l.PassTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.PassTypeId, l.Name })
            .IsUnique();
    }
}