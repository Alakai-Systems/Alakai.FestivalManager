namespace Alakai.FestivalManager.Application.Features.Registrations.Mapping;

public class RegistrationMappingProfile : Profile
{
    public RegistrationMappingProfile()
    {
        //Generics y Gets
        CreateMap<Registration, RegistrationDto>();
        CreateMap<IReadOnlyList<RegistrationDto>, IReadOnlyList<Registration>>();

        //Create Registration
        CreateMap<CreateRegistrationCommand, Registration>();
        CreateMap<CreateRegistrationRequest, CreateRegistrationCommand>();
        CreateMap<RegistrationDto, CreateRegistrationResponse>();

        //Update Registration
        CreateMap<UpdateRegistrationCommand, Registration>();
        CreateMap<UpdateRegistrationRequest, UpdateRegistrationCommand>();
        CreateMap<RegistrationDto, UpdateRegistrationResponse>();
    }
}