namespace Alakai.FestivalManager.Application.Features.PassTypes.Mappings;

public class PassTypeMappingProfile : Profile
{
    public PassTypeMappingProfile()
    {
        //Generics and Get
        CreateMap<PassType, PassTypeDto>();
        CreateMap<IReadOnlyList<PassTypeDto>, IReadOnlyList<PassType>>();

        //Create PassType
        CreateMap<CreatePassTypeRequest, CreatePassTypeCommand>();
        CreateMap<CreatePassTypeCommand, PassType>();
        CreateMap<PassTypeDto, CreatePassTypeResponse>();

        //Update PassType
        CreateMap<UpdatePassTypeRequest, UpdatePassTypeCommand>();
        CreateMap<UpdatePassTypeCommand, PassType>();
        CreateMap<PassTypeDto, UpdatePassTypeResponse>();
    }
}