namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Queries.GetItineraryById;

public class GetItineraryByIdHandler
{
    private readonly IRunnerItineraryRepository _itineraryRepository;
    private readonly IMapper _mapper;

    public GetItineraryByIdHandler(IRunnerItineraryRepository itineraryRepository, IMapper mapper)
    {
        _itineraryRepository = itineraryRepository;
        _mapper = mapper;
    }

    public async Task<RunnerItineraryDto?> HandleAsync(GetItineraryByIdQuery query, CancellationToken cancellationToken = default)
    {
        RunnerItinerary? itinerary = await _itineraryRepository.GetByIdAsync(query.Id, cancellationToken);

        if (itinerary is null)
        {
            return null;
        }

        return _mapper.Map<RunnerItineraryDto>(itinerary);
    }
}