namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class BusPassTypeConfiguration : IEntityTypeConfiguration<BusPassType>
{
    public void Configure(EntityTypeBuilder<BusPassType> builder)
    {
        builder.ToTable("BusPassTypes");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.BusId, p.PassTypeId }).IsUnique();

        builder.HasOne(p => p.PassType)
            .WithMany()
            .HasForeignKey(p => p.PassTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}