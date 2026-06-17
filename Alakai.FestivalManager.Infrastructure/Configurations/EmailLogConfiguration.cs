namespace Alakai.FestivalManager.Infrastructure.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("EmailLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TemplateKey).IsRequired();
        builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(200);
        builder.Property(e => e.RecipientName).HasMaxLength(200);
        builder.Property(e => e.Subject).IsRequired().HasMaxLength(300);
        builder.Property(e => e.BodyHtml).IsRequired();
        builder.Property(e => e.BodyText);
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);
        builder.Property(e => e.SentAt);
        builder.Property(e => e.IsActive).IsRequired();
        builder.HasIndex(e => e.EditionId);
        builder.HasIndex(e => e.EmailTemplateId);
        builder.HasIndex(e => e.RegistrationId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.TemplateKey);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RecipientEmail);
        builder.HasOne(e => e.Edition).WithMany().HasForeignKey(e => e.EditionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmailTemplate).WithMany().HasForeignKey(e => e.EmailTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Registration).WithMany().HasForeignKey(e => e.RegistrationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
