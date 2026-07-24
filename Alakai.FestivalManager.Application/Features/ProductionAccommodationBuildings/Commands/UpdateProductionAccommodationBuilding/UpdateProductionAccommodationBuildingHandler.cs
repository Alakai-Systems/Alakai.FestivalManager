namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Commands.UpdateProductionAccommodationBuilding;

public class UpdateProductionAccommodationBuildingHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public UpdateProductionAccommodationBuildingHandler(IProductionAccommodationBuildingRepository buildingRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationBuildingDto> HandleAsync(UpdateProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.Id, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{command.Id}' was not found.");
        }

        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        _mapper.Map(command, building);
        building.SetUpdated();

        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationBuildingDto>(building);
    }
}