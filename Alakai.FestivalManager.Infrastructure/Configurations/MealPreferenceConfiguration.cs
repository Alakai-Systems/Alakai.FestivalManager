namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class MealPreferenceConfiguration : IEntityTypeConfiguration<MealPreference>
{
    public void Configure(EntityTypeBuilder<MealPreference> builder)
    {
        builder.ToTable("MealPreferences");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.AllergiesNotes).HasMaxLength(1000);

        builder.HasIndex(m => m.RegistrationId).IsUnique();

        builder.HasOne(m => m.Registration)
            .WithMany()
            .HasForeignKey(m => m.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}