namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Contracts.Responses;

public class DeleteEmailTemplateResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}
