namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.ExpiresAt)
            .IsRequired();

        builder.Property(r => r.RevokedAt);

        builder.HasIndex(r => r.UserId);

        builder.HasIndex(r => r.Token)
            .IsUnique();

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}