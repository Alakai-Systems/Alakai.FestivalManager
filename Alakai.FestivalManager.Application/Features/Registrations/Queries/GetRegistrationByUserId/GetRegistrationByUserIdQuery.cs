namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationByUserId;

public class GetRegistrationByUserIdQuery
{
    public Guid Id { get; set; }

    public GetRegistrationByUserIdQuery(Guid id)
    {
        Id = id;
    }
}
