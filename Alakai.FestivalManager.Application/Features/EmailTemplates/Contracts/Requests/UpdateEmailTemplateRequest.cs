namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Contracts.Requests;

public class UpdateEmailTemplateRequest
{
    public Guid? EditionId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public string Language { get; set; } = "en";
}
