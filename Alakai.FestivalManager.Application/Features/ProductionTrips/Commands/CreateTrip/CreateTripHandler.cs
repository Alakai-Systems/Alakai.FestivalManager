namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Commands.CreateTrip;

public class CreateTripHandler
{
    private readonly IProductionTripRepository _tripRepository;
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public CreateTripHandler(IProductionTripRepository tripRepository, IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<ProductionTripDto> HandleAsync(CreateTripCommand command, CancellationToken cancellationToken = default)
    {
        ProductionPerson? person = await _productionPersonRepository.GetByIdAsync(command.ProductionPersonId, cancellationToken);

        if (person is null)
        {
            throw new NotFoundException($"Production person with id '{command.ProductionPersonId}' was not found.");
        }

        ProductionTrip trip = _mapper.Map<ProductionTrip>(command);

        await _tripRepository.AddAsync(trip, cancellationToken);
        await _tripRepository.SaveChangesAsync(cancellationToken);

        ProductionTrip created = await _tripRepository.GetByIdAsync(trip.Id, cancellationToken)
            ?? throw new NotFoundException($"Trip with id '{trip.Id}' was not found after creation.");

        return _mapper.Map<ProductionTripDto>(created);
    }
}