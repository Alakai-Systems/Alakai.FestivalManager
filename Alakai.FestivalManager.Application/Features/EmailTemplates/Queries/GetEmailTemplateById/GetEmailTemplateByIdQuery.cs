namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Queries.GetEmailTemplateById;

public class GetEmailTemplateByIdQuery
{
    public Guid Id { get; set; }

    public GetEmailTemplateByIdQuery(Guid id)
    {
        Id = id;
    }
}
