namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.CreateProductionAccommodationZone;

public class CreateProductionAccommodationZoneHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public CreateProductionAccommodationZoneHandler(IProductionAccommodationZoneRepository zoneRepository, IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationZoneDto> HandleAsync(CreateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.ProductionAccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{command.ProductionAccommodationBuildingId}' was not found.");
        }

        ProductionAccommodationZone zone = _mapper.Map<ProductionAccommodationZone>(command);

        await _zoneRepository.AddAsync(zone, cancellationToken);
        await _zoneRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationZoneDto>(zone);
    }
}