using Alakai.FestivalManager.Application.Features.Editions.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Editions.Mapping;

public class EditionMappingProfile : Profile
{
    public EditionMappingProfile()
    {
        //Generics y Gets
        CreateMap<Edition, EditionDto>();
        CreateMap<IReadOnlyList<EditionDto>, IReadOnlyList<Edition>>();

        //Create Edition
        CreateMap<CreateEditionRequest, CreateEditionCommand>();
        CreateMap<CreateEditionCommand, Edition>();
        CreateMap<EditionDto, CreateEditionResponse>();

        //Update Edition
        CreateMap<UpdateEditionRequest, UpdateEditionCommand>();
        CreateMap<UpdateEditionCommand, Edition>();
        CreateMap<EditionDto, UpdateEditionResponse>();
    }
}