namespace Alakai.FestivalManager.Admin.Contracts.EmailTemplates.Responses;

public class GetEmailTemplatesByEditionIdResponse
{
    public IReadOnlyList<EmailTemplateDto> EmailTemplates { get; set; } = [];
}
