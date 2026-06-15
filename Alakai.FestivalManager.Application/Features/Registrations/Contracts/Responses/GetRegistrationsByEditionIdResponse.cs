using System.Collections.Generic;
using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationsByEditionIdResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = new List<RegistrationDto>();
}
