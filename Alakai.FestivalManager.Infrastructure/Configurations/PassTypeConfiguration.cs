namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class PassTypeConfiguration : IEntityTypeConfiguration<PassType>
{
    public void Configure(EntityTypeBuilder<PassType> builder)
    {
        builder.ToTable("PassTypes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.SortOrder)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasOne(p => p.Edition)
            .WithMany(e => e.PassTypes)
            .HasForeignKey(p => p.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EditionId, p.Name })
            .IsUnique();
    }
}