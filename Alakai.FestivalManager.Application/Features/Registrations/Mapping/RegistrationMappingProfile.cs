namespace Alakai.FestivalManager.Application.Features.Registrations.Mapping;

public class RegistrationMappingProfile : Profile
{
    public RegistrationMappingProfile()
    {
        CreateMap<CreateRegistrationCommand, Registration>();
        CreateMap<UpdateRegistrationCommand, Registration>();
        CreateMap<Registration, RegistrationDto>();
    }
}
