namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Queries.GetProductionAccommodationZoneById;

public class GetProductionAccommodationZoneByIdHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationZoneByIdHandler(IProductionAccommodationZoneRepository zoneRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationZoneDto?> HandleAsync(GetProductionAccommodationZoneByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(query.Id, cancellationToken);

        if (zone is null)
        {
            return null;
        }

        return _mapper.Map<ProductionAccommodationZoneDto>(zone);
    }
}