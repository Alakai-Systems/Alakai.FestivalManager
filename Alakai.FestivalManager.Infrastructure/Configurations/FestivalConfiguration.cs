namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class FestivalConfiguration : IEntityTypeConfiguration<Festival>
{
    public void Configure(EntityTypeBuilder<Festival> builder)
    {
        builder.ToTable("Festivals");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(f => f.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(f => f.Slug)
            .IsUnique();

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.Website)
            .HasMaxLength(300);

        builder.Property(f => f.LogoUrl)
            .HasMaxLength(500);

        builder.Property(f => f.IsActive)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt);
    }
}
