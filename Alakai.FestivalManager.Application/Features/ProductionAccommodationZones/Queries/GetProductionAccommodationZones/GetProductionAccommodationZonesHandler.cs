namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Queries.GetProductionAccommodationZones;

public class GetProductionAccommodationZonesHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationZonesHandler(IProductionAccommodationZoneRepository zoneRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationZoneDto>> HandleAsync(GetProductionAccommodationZonesQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodationZone> zones = await _zoneRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationZoneDto>>(zones);
    }
}