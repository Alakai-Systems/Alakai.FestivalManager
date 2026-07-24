namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Commands.DeleteItinerary;

public class DeleteItineraryHandler
{
    private readonly IRunnerItineraryRepository _itineraryRepository;

    public DeleteItineraryHandler(IRunnerItineraryRepository itineraryRepository)
    {
        _itineraryRepository = itineraryRepository;
    }

    public async Task<Guid> HandleAsync(DeleteItineraryCommand command, CancellationToken cancellationToken = default)
    {
        RunnerItinerary? itinerary = await _itineraryRepository.GetByIdAsync(command.Id, cancellationToken);

        if (itinerary is null)
        {
            throw new NotFoundException($"Itinerary with id '{command.Id}' was not found.");
        }

        _itineraryRepository.Delete(itinerary);
        await _itineraryRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}