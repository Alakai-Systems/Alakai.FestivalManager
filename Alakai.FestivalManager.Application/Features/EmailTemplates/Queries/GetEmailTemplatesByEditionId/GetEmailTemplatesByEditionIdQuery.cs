namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Queries.GetEmailTemplatesByEditionId;

public class GetEmailTemplatesByEditionIdQuery
{
    public Guid EditionId { get; set; }

    public GetEmailTemplatesByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
