namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class EditionConfiguration : IEntityTypeConfiguration<Edition>
{
    public void Configure(EntityTypeBuilder<Edition> builder)
    {
        builder.ToTable("Editions");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Year)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .IsRequired();

        builder.Property(e => e.EndDate)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasOne(e => e.Festival)
            .WithMany(f => f.Editions)
            .HasForeignKey(e => e.FestivalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.FestivalId, e.Year })
            .IsUnique();
    }
}