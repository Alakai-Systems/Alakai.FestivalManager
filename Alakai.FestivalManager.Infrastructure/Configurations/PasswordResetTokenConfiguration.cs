namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.ExpiresAt)
            .IsRequired();

        builder.Property(p => p.UsedAt);

        builder.HasIndex(p => p.UserId);

        builder.HasIndex(p => p.Token)
            .IsUnique();

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
