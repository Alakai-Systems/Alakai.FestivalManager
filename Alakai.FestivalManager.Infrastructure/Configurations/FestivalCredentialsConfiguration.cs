namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class FestivalCredentialsConfiguration : IEntityTypeConfiguration<FestivalCredentials>
{
    public void Configure(EntityTypeBuilder<FestivalCredentials> builder)
    {
        builder.ToTable("FestivalCredentials");

        builder.HasKey(fc => fc.Id);

        builder.HasIndex(fc => fc.FestivalId)
            .IsUnique();

        builder.Property(fc => fc.RedsysMerchantCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(fc => fc.RedsysTerminal)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(fc => fc.RedsysSecretKey)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fc => fc.RedsysMerchantName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(fc => fc.RedsysPaymentUrl)
            .HasMaxLength(300);

        builder.Property(fc => fc.EmailHost)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fc => fc.EmailPort)
            .IsRequired();

        builder.Property(fc => fc.EmailUsername)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fc => fc.EmailPassword)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fc => fc.EmailFromEmail)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fc => fc.EmailFromName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(fc => fc.EmailUseSSL)
            .IsRequired();

        builder.HasOne(fc => fc.Festival)
            .WithOne(f => f.Credentials)
            .HasForeignKey<FestivalCredentials>(fc => fc.FestivalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}