namespace Alakai.FestivalManager.Admin.Contracts.EmailTemplates.Responses;

public class GetEmailTemplatesResponse
{
    public IReadOnlyList<EmailTemplateDto> EmailTemplates { get; set; } = [];
}
