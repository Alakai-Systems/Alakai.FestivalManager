namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class InvoiceTemplateConfiguration : IEntityTypeConfiguration<InvoiceTemplate>
{
    public void Configure(EntityTypeBuilder<InvoiceTemplate> builder)
    {
        builder.ToTable("InvoiceTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.CompanyName).HasMaxLength(200);
        builder.Property(t => t.TaxId).HasMaxLength(50);
        builder.Property(t => t.Address).HasMaxLength(300);
        builder.Property(t => t.City).HasMaxLength(100);
        builder.Property(t => t.PostalCode).HasMaxLength(20);
        builder.Property(t => t.Country).HasMaxLength(100);
        builder.Property(t => t.LogoUrl).HasMaxLength(500);

        builder.HasOne(t => t.Edition)
            .WithMany()
            .HasForeignKey(t => t.EditionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}