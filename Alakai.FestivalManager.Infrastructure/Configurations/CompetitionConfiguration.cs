namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competitions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.EditionId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.Property(c => c.Format)
            .IsRequired();

        builder.Property(c => c.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.RequiresPartner)
            .IsRequired();

        builder.Property(c => c.RequiresRole)
            .IsRequired();

        builder.Property(c => c.SortOrder)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.HasIndex(c => c.EditionId);

        builder.HasIndex(c => new { c.EditionId, c.Name })
            .IsUnique();

        builder.HasOne(c => c.Edition)
            .WithMany()
            .HasForeignKey(c => c.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
