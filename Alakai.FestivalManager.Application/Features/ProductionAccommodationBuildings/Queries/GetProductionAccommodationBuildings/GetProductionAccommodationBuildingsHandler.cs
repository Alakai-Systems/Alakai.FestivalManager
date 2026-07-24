namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Queries.GetProductionAccommodationBuildings;

public class GetProductionAccommodationBuildingsHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationBuildingsHandler(IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationBuildingDto>> HandleAsync(GetProductionAccommodationBuildingsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodationBuilding> buildings = await _buildingRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationBuildingDto>>(buildings);
    }
}