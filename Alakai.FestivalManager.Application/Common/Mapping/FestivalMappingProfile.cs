
namespace Alakai.FestivalManager.Application.Common.Mappings;

public class FestivalMappingProfile : Profile
{
    public FestivalMappingProfile()
    {
        //Generics y Gets
        CreateMap<Festival, FestivalDto>();
        CreateMap<IReadOnlyList<FestivalDto>, IReadOnlyList<Festival>>();

        //Create Festival
        CreateMap<CreateFestivalCommand, Festival>();
        CreateMap<CreateFestivalRequest, CreateFestivalCommand>();
        CreateMap<FestivalDto, CreateFestivalResponse>();

        //Update Festival
        CreateMap<UpdateFestivalRequest, UpdateFestivalCommand>();
        CreateMap<UpdateFestivalCommand, Festival>();
        CreateMap<FestivalDto, UpdateFestivalResponse>();
    }
}