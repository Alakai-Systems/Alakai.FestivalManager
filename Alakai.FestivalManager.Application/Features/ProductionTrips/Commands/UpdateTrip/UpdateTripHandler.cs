namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Commands.UpdateTrip;

public class UpdateTripHandler
{
    private readonly IProductionTripRepository _tripRepository;
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public UpdateTripHandler(IProductionTripRepository tripRepository, IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<ProductionTripDto> HandleAsync(UpdateTripCommand command, CancellationToken cancellationToken = default)
    {
        ProductionTrip? trip = await _tripRepository.GetByIdAsync(command.Id, cancellationToken);

        if (trip is null)
        {
            throw new NotFoundException($"Trip with id '{command.Id}' was not found.");
        }

        ProductionPerson? person = await _productionPersonRepository.GetByIdAsync(command.ProductionPersonId, cancellationToken);

        if (person is null)
        {
            throw new NotFoundException($"Production person with id '{command.ProductionPersonId}' was not found.");
        }

        _mapper.Map(command, trip);
        trip.SetUpdated();

        await _tripRepository.SaveChangesAsync(cancellationToken);

        ProductionTrip updated = await _tripRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException($"Trip with id '{command.Id}' was not found after update.");

        return _mapper.Map<ProductionTripDto>(updated);
    }
}