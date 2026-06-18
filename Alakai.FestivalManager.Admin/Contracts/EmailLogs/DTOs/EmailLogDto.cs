namespace Alakai.FestivalManager.Admin.Contracts.EmailLogs.DTOs;

public class EmailLogDto
{
    public Guid Id { get; set; }
    public Guid? EditionId { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public Guid? RegistrationId { get; set; }
    public Guid? UserId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string? RecipientName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public EmailLogStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsActive { get; set; }
}
