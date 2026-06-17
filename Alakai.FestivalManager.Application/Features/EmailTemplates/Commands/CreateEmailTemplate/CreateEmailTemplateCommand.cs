namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Commands.CreateEmailTemplate;

public class CreateEmailTemplateCommand
{
    public Guid? EditionId { get; set; }
    public EmailTemplateKey TemplateKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public bool IsSystem { get; set; }
}
