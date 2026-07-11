namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EditionId);

        builder.Property(e => e.TemplateKey)
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.BodyHtml)
            .IsRequired();

        builder.Property(e => e.BodyText);

        builder.Property(e => e.IsSystem)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);

        builder.Property(e => e.Language)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("en");

        builder.HasIndex(e => e.EditionId);

        builder.HasIndex(e => new { e.EditionId, e.TemplateKey, e.Language })
            .IsUnique();

        builder.HasOne(e => e.Edition)
            .WithMany()
            .HasForeignKey(e => e.EditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
