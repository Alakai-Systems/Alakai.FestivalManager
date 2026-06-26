namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationById;

public class GetRegistrationByUserIdQuery
{
    public Guid Id { get; set; }

    public GetRegistrationByUserIdQuery(Guid id)
    {
        Id = id;
    }
}
