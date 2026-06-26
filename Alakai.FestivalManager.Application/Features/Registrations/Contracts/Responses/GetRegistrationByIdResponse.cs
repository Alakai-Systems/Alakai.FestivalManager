namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;

public class GetRegistrationByIdResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
