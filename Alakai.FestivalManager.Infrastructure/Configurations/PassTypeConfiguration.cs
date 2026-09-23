namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class PassTypeConfiguration : IEntityTypeConfiguration<PassType>
{
    public void Configure(EntityTypeBuilder<PassType> builder)
    {
        builder.ToTable("PassTypes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.SortOrder)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        // Por defecto los 4 modulos activos (Competitions=1, Accommodation=2, Transport=4, Meals=8
        // -> 15), para que los pases ya existentes no pierdan visibilidad de ningun modulo al
        // aplicar la migracion.
        builder.Property(p => p.EnabledModules)
            .IsRequired()
            .HasDefaultValue(FestivalModule.Competitions | FestivalModule.Accommodation | FestivalModule.Transport | FestivalModule.Meals);

        builder.HasOne(p => p.Edition)
            .WithMany(e => e.PassTypes)
            .HasForeignKey(p => p.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.EditionId, p.Name })
            .IsUnique();
    }
}