namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionSupplierConfiguration : IEntityTypeConfiguration<ProductionSupplier>
{
    public void Configure(EntityTypeBuilder<ProductionSupplier> builder)
    {
        builder.ToTable("ProductionSuppliers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ServiceType).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ContactName).HasMaxLength(150);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Notes).HasMaxLength(1000);
        builder.Property(s => s.IsActive).IsRequired();

        builder.HasOne(s => s.Edition)
            .WithMany()
            .HasForeignKey(s => s.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}