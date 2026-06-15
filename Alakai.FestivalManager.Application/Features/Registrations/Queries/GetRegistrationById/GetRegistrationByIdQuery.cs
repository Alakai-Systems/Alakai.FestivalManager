using System;

namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationById;

public class GetRegistrationByIdQuery
{
    public Guid Id { get; set; }

    public GetRegistrationByIdQuery(Guid id)
    {
        Id = id;
    }
}
