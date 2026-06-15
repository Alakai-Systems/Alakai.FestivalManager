using System;

namespace Alakai.FestivalManager.Application.Features.Registrations.Queries.GetRegistrationsByEditionId;

public class GetRegistrationsByEditionIdQuery
{
    public Guid EditionId { get; set; }

    public GetRegistrationsByEditionIdQuery(Guid editionId)
    {
        EditionId = editionId;
    }
}
