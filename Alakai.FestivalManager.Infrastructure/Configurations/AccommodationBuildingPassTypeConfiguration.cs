namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class AccommodationBuildingPassTypeConfiguration : IEntityTypeConfiguration<AccommodationBuildingPassType>
{
    public void Configure(EntityTypeBuilder<AccommodationBuildingPassType> builder)
    {
        builder.ToTable("AccommodationBuildingPassTypes");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.AccommodationBuildingId, p.PassTypeId }).IsUnique();

        builder.HasOne(p => p.PassType)
            .WithMany()
            .HasForeignKey(p => p.PassTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}