namespace Alakai.FestivalManager.Admin.Contracts.Registrations.Responses;

public class GetRegistrationsResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = [];
}
