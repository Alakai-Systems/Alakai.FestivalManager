namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.Responses;
public class GetRegistrationByUserIdResponse
{
    public RegistrationDto Registration { get; set; } = new RegistrationDto();
}
