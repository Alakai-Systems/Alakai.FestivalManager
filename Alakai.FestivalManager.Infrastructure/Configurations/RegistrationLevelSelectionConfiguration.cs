namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class RegistrationLevelSelectionConfiguration : IEntityTypeConfiguration<RegistrationLevelSelection>
{
    public void Configure(EntityTypeBuilder<RegistrationLevelSelection> builder)
    {
        builder.ToTable("RegistrationLevelSelections");

        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Registration)
            .WithMany(r => r.LevelSelections)
            .HasForeignKey(s => s.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Level)
            .WithMany()
            .HasForeignKey(s => s.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.RegistrationId, s.LevelId })
            .IsUnique();
    }
}
