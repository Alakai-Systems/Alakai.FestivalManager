namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Queries.GetBuildingById;

public class GetBuildingByIdHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetBuildingByIdHandler(IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationBuildingDto?> HandleAsync(GetBuildingByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(query.Id, cancellationToken);

        if (building is null)
        {
            return null;
        }

        return _mapper.Map<ProductionAccommodationBuildingDto>(building);
    }
}