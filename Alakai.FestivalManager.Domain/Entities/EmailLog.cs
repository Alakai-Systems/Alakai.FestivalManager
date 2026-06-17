namespace Alakai.FestivalManager.Domain.Entities;

public class EmailLog : BaseEntity
{
    public Guid? EditionId { get; set; }
    public Edition? Edition { get; set; }

    public Guid? EmailTemplateId { get; set; }
    public EmailTemplate? EmailTemplate { get; set; }

    public Guid? RegistrationId { get; set; }
    public Registration? Registration { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; } = true;
}
