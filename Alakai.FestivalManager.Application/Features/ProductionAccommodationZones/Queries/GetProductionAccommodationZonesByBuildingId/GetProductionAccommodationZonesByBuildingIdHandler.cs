namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Queries.GetProductionAccommodationZonesByBuildingId;

public class GetProductionAccommodationZonesByBuildingIdHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationZonesByBuildingIdHandler(IProductionAccommodationZoneRepository zoneRepository, IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationZoneDto>> HandleAsync(GetProductionAccommodationZonesByBuildingIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(query.ProductionAccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{query.ProductionAccommodationBuildingId}' was not found.");
        }

        IReadOnlyList<ProductionAccommodationZone> zones = await _zoneRepository.GetByBuildingIdAsync(query.ProductionAccommodationBuildingId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationZoneDto>>(zones);
    }
}