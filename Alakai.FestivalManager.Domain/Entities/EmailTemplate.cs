namespace Alakai.FestivalManager.Domain.Entities;

public class EmailTemplate : BaseEntity
{
    public Guid? EditionId { get; set; }
    public Edition? Edition { get; set; }

    public EmailTemplateKey TemplateKey { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }

    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>ISO 639-1 language code: "en", "es", "fr". Null means applies to all languages.</summary>
    public string Language { get; set; } = "en";
}
