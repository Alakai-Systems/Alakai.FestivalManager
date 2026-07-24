namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Queries.GetProductionAccommodationBuildingById;

public class GetProductionAccommodationBuildingByIdHandler
{
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetProductionAccommodationBuildingByIdHandler(IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<ProductionAccommodationBuildingDto?> HandleAsync(GetProductionAccommodationBuildingByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(query.Id, cancellationToken);

        if (building is null)
        {
            return null;
        }

        return _mapper.Map<ProductionAccommodationBuildingDto>(building);
    }
}