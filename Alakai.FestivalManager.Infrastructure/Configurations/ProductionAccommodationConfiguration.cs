namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class ProductionAccommodationConfiguration : IEntityTypeConfiguration<ProductionAccommodation>
{
    public void Configure(EntityTypeBuilder<ProductionAccommodation> builder)
    {
        builder.ToTable("ProductionAccommodations");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);
    }
}