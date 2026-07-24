namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Commands.DeleteTrip;

public class DeleteTripHandler
{
    private readonly IProductionTripRepository _tripRepository;

    public DeleteTripHandler(IProductionTripRepository tripRepository)
    {
        _tripRepository = tripRepository;
    }

    public async Task<Guid> HandleAsync(DeleteTripCommand command, CancellationToken cancellationToken = default)
    {
        ProductionTrip? trip = await _tripRepository.GetByIdAsync(command.Id, cancellationToken);

        if (trip is null)
        {
            throw new NotFoundException($"Trip with id '{command.Id}' was not found.");
        }

        _tripRepository.Delete(trip);
        await _tripRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}