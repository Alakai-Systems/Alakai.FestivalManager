namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Queries.GetTripById;

public class GetTripByIdHandler
{
    private readonly IProductionTripRepository _tripRepository;
    private readonly IMapper _mapper;

    public GetTripByIdHandler(IProductionTripRepository tripRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _mapper = mapper;
    }

    public async Task<ProductionTripDto?> HandleAsync(GetTripByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionTrip? trip = await _tripRepository.GetByIdAsync(query.Id, cancellationToken);

        if (trip is null)
        {
            return null;
        }

        return _mapper.Map<ProductionTripDto>(trip);
    }
}