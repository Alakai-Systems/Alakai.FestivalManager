namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Commands.CreateItinerary;

public class CreateItineraryHandler
{
    private readonly IRunnerItineraryRepository _itineraryRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateItineraryHandler(IRunnerItineraryRepository itineraryRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _itineraryRepository = itineraryRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<RunnerItineraryDto> HandleAsync(CreateItineraryCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        RunnerItinerary itinerary = _mapper.Map<RunnerItinerary>(command);

        await _itineraryRepository.AddAsync(itinerary, cancellationToken);
        await _itineraryRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<RunnerItineraryDto>(itinerary);
    }
}