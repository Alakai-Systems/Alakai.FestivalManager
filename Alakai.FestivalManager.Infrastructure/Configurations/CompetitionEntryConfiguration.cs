namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class CompetitionEntryConfiguration : IEntityTypeConfiguration<CompetitionEntry>
{
    public void Configure(EntityTypeBuilder<CompetitionEntry> builder)
    {
        builder.ToTable("CompetitionEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CompetitionId)
            .IsRequired();

        builder.Property(e => e.RegistrationId)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.Property(e => e.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.Status)
            .IsRequired();

        builder.Property(e => e.CancelledAt);

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.HasIndex(e => e.CompetitionId);

        builder.HasIndex(e => e.RegistrationId);

        builder.HasIndex(e => e.PartnerRegistrationId);

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => new { e.CompetitionId, e.RegistrationId })
            .IsUnique();

        builder.HasOne(e => e.Competition)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Registration)
            .WithMany()
            .HasForeignKey(e => e.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PartnerRegistration)
            .WithMany()
            .HasForeignKey(e => e.PartnerRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
