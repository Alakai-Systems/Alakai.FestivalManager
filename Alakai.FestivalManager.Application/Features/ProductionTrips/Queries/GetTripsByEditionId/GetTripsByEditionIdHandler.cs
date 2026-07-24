namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Queries.GetTripsByEditionId;

public class GetTripsByEditionIdHandler
{
    private readonly IProductionTripRepository _tripRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetTripsByEditionIdHandler(IProductionTripRepository tripRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _tripRepository = tripRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionTripDto>> HandleAsync(GetTripsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<ProductionTrip> trips = await _tripRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionTripDto>>(trips);
    }
}