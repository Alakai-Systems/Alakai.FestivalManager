namespace Alakai.FestivalManager.Admin.Contracts.Registrations.Responses;

public class GetRegistrationsByEditionIdResponse
{
    public IReadOnlyList<RegistrationDto> Registrations { get; set; } = [];
}
