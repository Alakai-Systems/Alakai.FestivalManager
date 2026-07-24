namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Mappings;

public class ProductionAccommodationBuildingMappingProfile : Profile
{
    public ProductionAccommodationBuildingMappingProfile()
    {
        CreateMap<ProductionAccommodationBuilding, ProductionAccommodationBuildingDto>();
        CreateMap<IReadOnlyList<ProductionAccommodationBuildingDto>, IReadOnlyList<ProductionAccommodationBuilding>>();

        CreateMap<CreateProductionAccommodationBuildingRequest, CreateProductionAccommodationBuildingCommand>();
        CreateMap<CreateProductionAccommodationBuildingCommand, ProductionAccommodationBuilding>();
        CreateMap<ProductionAccommodationBuildingDto, CreateProductionAccommodationBuildingResponse>();

        CreateMap<UpdateProductionAccommodationBuildingRequest, UpdateProductionAccommodationBuildingCommand>();
        CreateMap<UpdateProductionAccommodationBuildingCommand, ProductionAccommodationBuilding>();
        CreateMap<ProductionAccommodationBuildingDto, UpdateProductionAccommodationBuildingResponse>();
    }
}