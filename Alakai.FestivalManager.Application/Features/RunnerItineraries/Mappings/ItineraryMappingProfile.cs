namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Mappings;

public class ItineraryMappingProfile : Profile
{
    public ItineraryMappingProfile()
    {
        CreateMap<RunnerItinerary, RunnerItineraryDto>();
        CreateMap<IReadOnlyList<RunnerItineraryDto>, IReadOnlyList<RunnerItinerary>>();

        CreateMap<CreateItineraryRequest, CreateItineraryCommand>();
        CreateMap<CreateItineraryCommand, RunnerItinerary>();
        CreateMap<RunnerItineraryDto, CreateItineraryResponse>();

        CreateMap<UpdateItineraryRequest, UpdateItineraryCommand>();
        CreateMap<UpdateItineraryCommand, RunnerItinerary>();
        CreateMap<RunnerItineraryDto, UpdateItineraryResponse>();
    }
}