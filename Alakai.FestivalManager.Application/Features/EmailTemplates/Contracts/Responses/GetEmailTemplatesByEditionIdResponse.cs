namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Contracts.Responses;

public class GetEmailTemplatesByEditionIdResponse
{
    public IReadOnlyList<EmailTemplateDto> EmailTemplates { get; set; } = [];
}
