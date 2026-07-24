namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Queries.GetBuildings;

public class GetBuildingsHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetBuildingsHandler(IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductionAccommodationBuildingDto>> HandleAsync(GetBuildingsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodationBuilding> buildings = await _buildingRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ProductionAccommodationBuildingDto>>(buildings);
    }
}