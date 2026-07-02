namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class InvoiceSettingsConfiguration : IEntityTypeConfiguration<InvoiceSettings>
{
    public void Configure(EntityTypeBuilder<InvoiceSettings> builder)
    {
        builder.ToTable("InvoiceSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName).HasMaxLength(200);
        builder.Property(s => s.TaxId).HasMaxLength(50);
        builder.Property(s => s.Address).HasMaxLength(300);
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
    }
}