namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Queries.GetProductionAccommodationsByZoneId;

public class GetProductionAccommodationsByZoneIdHandler
{
    private readonly IProductionAccommodationRepository _accommodationRepository;
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationsByZoneIdHandler(IProductionAccommodationRepository accommodationRepository, IProductionAccommodationZoneRepository zoneRepository, IMapper mapper)
    {
        _accommodationRepository = accommodationRepository;
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationDto>> HandleAsync(GetProductionAccommodationsByZoneIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(query.ProductionAccommodationZoneId, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Production accommodation zone with id '{query.ProductionAccommodationZoneId}' was not found.");
        }

        IReadOnlyList<ProductionAccommodation> accommodations = await _accommodationRepository.GetByZoneIdAsync(query.ProductionAccommodationZoneId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationDto>>(accommodations);
    }
}