namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionPersonConfiguration : IEntityTypeConfiguration<ProductionPerson>
{
    public void Configure(EntityTypeBuilder<ProductionPerson> builder)
    {
        builder.ToTable("ProductionPeople");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Category)
            .IsRequired();

        builder.Property(p => p.RoleTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Phone)
            .HasMaxLength(30);

        builder.Property(p => p.DocumentType)
            .IsRequired();

        builder.Property(p => p.DocumentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Nationality)
            .HasMaxLength(100);

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasOne(p => p.Edition)
            .WithMany()
            .HasForeignKey(p => p.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EditionId, p.DocumentNumber })
            .IsUnique();
    }
}