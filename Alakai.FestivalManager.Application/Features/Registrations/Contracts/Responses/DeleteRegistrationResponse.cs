using System;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class DeleteRegistrationResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
