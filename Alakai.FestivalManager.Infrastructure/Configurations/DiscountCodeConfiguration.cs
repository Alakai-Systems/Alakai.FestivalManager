namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class DiscountCodeConfiguration : IEntityTypeConfiguration<DiscountCode>
{
    public void Configure(EntityTypeBuilder<DiscountCode> builder)
    {
        builder.ToTable("DiscountCodes");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.EditionId)
            .IsRequired();

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.DiscountType)
            .IsRequired();

        builder.Property(d => d.DiscountValue)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.ActivationType)
            .IsRequired();

        builder.Property(d => d.ActivationThreshold);

        builder.Property(d => d.MaxUses);

        builder.Property(d => d.CurrentUses)
            .IsRequired();

        builder.Property(d => d.StartsAt);

        builder.Property(d => d.EndsAt);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasIndex(d => d.EditionId);

        builder.HasIndex(d => new { d.EditionId, d.Code })
            .IsUnique();

        builder.HasOne(d => d.Edition)
            .WithMany()
            .HasForeignKey(d => d.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
