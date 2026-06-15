using Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class UpdateRegistrationResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
