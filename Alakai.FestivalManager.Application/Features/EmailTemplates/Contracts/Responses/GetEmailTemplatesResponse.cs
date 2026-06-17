namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Contracts.Responses;

public class GetEmailTemplatesResponse
{
    public IReadOnlyList<EmailTemplateDto> EmailTemplates { get; set; } = [];
}
