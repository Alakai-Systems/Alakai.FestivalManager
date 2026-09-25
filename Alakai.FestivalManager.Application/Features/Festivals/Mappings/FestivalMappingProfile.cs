namespace Alakai.FestivalManager.Application.Features.Festivals.Mappings;

public class FestivalMappingProfile : Profile
{
    public FestivalMappingProfile()
    {
        //Generics y Gets
        CreateMap<Festival, FestivalDto>()
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src =>
                src.Credentials != null && !string.IsNullOrWhiteSpace(src.Credentials.Currency) ? src.Credentials.Currency : "EUR"));
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