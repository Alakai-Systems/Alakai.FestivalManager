namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Commands.CreateBuilding;

public class CreateBuildingHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public CreateBuildingHandler(IProductionAccommodationBuildingRepository buildingRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationBuildingDto> HandleAsync(CreateBuildingCommand command, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(command.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{command.EditionId}' was not found.");
        }

        ProductionAccommodationBuilding building = _mapper.Map<ProductionAccommodationBuilding>(command);

        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductionAccommodationBuildingDto>(building);
    }
}