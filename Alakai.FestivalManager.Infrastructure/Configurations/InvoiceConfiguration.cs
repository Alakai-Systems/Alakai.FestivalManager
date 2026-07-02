namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(i => i.Number)
            .IsUnique();

        builder.HasIndex(i => i.RegistrationId)
            .IsUnique();

        builder.Property(i => i.Amount)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.BaseAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.VatRate)
            .HasColumnType("decimal(5,2)");

        builder.Property(i => i.VatAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.FiscalName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.TaxId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(i => i.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(i => i.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PdfUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(i => i.Registration)
            .WithMany()
            .HasForeignKey(i => i.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}