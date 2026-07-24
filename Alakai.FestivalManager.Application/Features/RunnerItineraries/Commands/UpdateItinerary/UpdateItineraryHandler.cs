namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Commands.UpdateItinerary;

public class UpdateItineraryHandler
{
    private readonly IRunnerItineraryRepository _itineraryRepository;
    private readonly IMapper _mapper;

    public UpdateItineraryHandler(IRunnerItineraryRepository itineraryRepository, IMapper mapper)
    {
        _itineraryRepository = itineraryRepository;
        _mapper = mapper;
    }

    public async Task<RunnerItineraryDto> HandleAsync(UpdateItineraryCommand command, CancellationToken cancellationToken = default)
    {
        RunnerItinerary? itinerary = await _itineraryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (itinerary is null)
        {
            throw new NotFoundException($"Itinerary with id '{command.Id}' was not found.");
        }

        _mapper.Map(command, itinerary);
        itinerary.SetUpdated();

        await _itineraryRepository.SaveChangesAsync(cancellationToken);

        RunnerItinerary updated = await _itineraryRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Itinerary with id '{command.Id}' was not found after update.");

        return _mapper.Map<RunnerItineraryDto>(updated);
    }
}