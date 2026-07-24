namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Commands.UpdateProductionAccommodationZone;

public class UpdateProductionAccommodationZoneHandler
{
    private readonly IProductionAccommodationZoneRepository _zoneRepository;
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public UpdateProductionAccommodationZoneHandler(IProductionAccommodationZoneRepository zoneRepository, IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _zoneRepository = zoneRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationZoneDto> HandleAsync(UpdateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationZone? zone = await _zoneRepository.GetByIdAsync(command.Id, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Production accommodation zone with id '{command.Id}' was not found.");
        }

        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.ProductionAccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{command.ProductionAccommodationBuildingId}' was not found.");
        }

        _mapper.Map(command, zone);
        zone.SetUpdated();

        await _zoneRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationZoneDto>(zone);
    }
}