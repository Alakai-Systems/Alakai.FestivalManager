namespace Alakai.FestivalManager.Infrastructure.Persistence.Configurations;

public class EmailLayoutConfiguration : IEntityTypeConfiguration<EmailLayout>
{
    public void Configure(EntityTypeBuilder<EmailLayout> builder)
    {
        builder.ToTable("EmailLayouts");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HeaderHtml).IsRequired();
        builder.Property(e => e.HeaderText);
        builder.Property(e => e.FooterHtml).IsRequired();
        builder.Property(e => e.FooterText);
        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
    }
}