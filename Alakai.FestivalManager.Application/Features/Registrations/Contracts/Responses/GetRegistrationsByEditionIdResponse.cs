namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationsByEditionIdResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = new List<RegistrationDto>();
}
