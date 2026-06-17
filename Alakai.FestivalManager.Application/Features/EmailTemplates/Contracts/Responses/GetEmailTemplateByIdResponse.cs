namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Contracts.Responses;

public class GetEmailTemplateByIdResponse
{
    public EmailTemplateDto EmailTemplate { get; set; } = default!;
}
