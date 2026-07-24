namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Queries.GetBuildingsByEditionId;

public class GetBuildingsByEditionIdHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IEditionRepository _editionRepository;
    private readonly IMapper _mapper;

    public GetBuildingsByEditionIdHandler(IProductionAccommodationBuildingRepository buildingRepository, IEditionRepository editionRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _editionRepository = editionRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationBuildingDto>> HandleAsync(GetBuildingsByEditionIdQuery query, CancellationToken cancellationToken = default)
    {
        Edition? edition = await _editionRepository.GetByIdAsync(query.EditionId, cancellationToken);

        if (edition is null)
        {
            throw new NotFoundException($"Edition with id '{query.EditionId}' was not found.");
        }

        IReadOnlyList<ProductionAccommodationBuilding> buildings = await _buildingRepository.GetByEditionIdAsync(query.EditionId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationBuildingDto>>(buildings);
    }
}