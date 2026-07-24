namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Mappings;

public class ProductionAccommodationBuildingMappingProfile : Profile
{
    public ProductionAccommodationBuildingMappingProfile()
    {
        CreateMap<ProductionAccommodationBuilding, ProductionAccommodationBuildingDto>();
        CreateMap<IReadOnlyList<ProductionAccommodationBuildingDto>, IReadOnlyList<ProductionAccommodationBuilding>>();

        CreateMap<CreateProductionAccommodationBuildingRequest, CreateBuildingCommand>();
        CreateMap<CreateBuildingCommand, ProductionAccommodationBuilding>();
        CreateMap<ProductionAccommodationBuildingDto, CreateProductionAccommodationBuildingResponse>();

        CreateMap<UpdateProductionAccommodationBuildingRequest, UpdateBuildingCommand>();
        CreateMap<UpdateBuildingCommand, ProductionAccommodationBuilding>();
        CreateMap<ProductionAccommodationBuildingDto, UpdateProductionAccommodationBuildingResponse>();
    }
}