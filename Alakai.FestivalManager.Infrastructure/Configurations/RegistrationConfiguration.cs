namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.EditionId)
            .IsRequired();

        builder.Property(r => r.PassTypeId)
            .IsRequired();

        builder.Property(r => r.LevelId);

        builder.Property(r => r.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Phone)
            .HasMaxLength(50);

        builder.Property(r => r.Country)
            .HasMaxLength(100);

        builder.Property(r => r.City)
            .HasMaxLength(100);

        builder.Property(r => r.DanceRole);

        builder.Property(r => r.PartnerEmail)
            .HasMaxLength(200);

        builder.Property(r => r.PartnerRegistrationId);

        builder.Property(r => r.Status)
            .IsRequired();

        builder.Property(r => r.PaymentStatus)
            .IsRequired();

        builder.Property(r => r.BasePrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DiscountAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.FinalPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DiscountCode)
            .HasMaxLength(100);

        builder.Property(r => r.PaymentReference)
            .HasMaxLength(200);

        builder.Property(r => r.PaidAt);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.Property(r => r.InternalNotes)
            .HasMaxLength(2000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.CancelledAt);

        builder.Property(r => r.IsActive)
            .IsRequired();

        builder.HasIndex(r => r.EditionId);

        builder.HasIndex(r => r.PassTypeId);

        builder.HasIndex(r => r.LevelId);

        builder.HasIndex(r => r.Email);

        builder.HasIndex(r => new { r.EditionId, r.Email })
            .IsUnique(false);

        builder.HasIndex(r => r.Status);

        builder.HasIndex(r => r.PaymentStatus);

        builder.HasIndex(r => r.PartnerEmail);

        builder.HasOne(r => r.Edition)
            .WithMany()
            .HasForeignKey(r => r.EditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PassType)
            .WithMany()
            .HasForeignKey(r => r.PassTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Level)
            .WithMany()
            .HasForeignKey(r => r.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PartnerRegistration)
            .WithMany()
            .HasForeignKey(r => r.PartnerRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}