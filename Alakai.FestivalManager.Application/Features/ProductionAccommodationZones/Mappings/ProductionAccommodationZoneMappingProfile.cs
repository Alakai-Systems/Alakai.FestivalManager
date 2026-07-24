namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Mappings;

public class ProductionAccommodationZoneMappingProfile : Profile
{
    public ProductionAccommodationZoneMappingProfile()
    {
        CreateMap<ProductionAccommodationZone, ProductionAccommodationZoneDto>();
        CreateMap<IReadOnlyList<ProductionAccommodationZoneDto>, IReadOnlyList<ProductionAccommodationZone>>();

        CreateMap<CreateProductionAccommodationZoneRequest, CreateProductionAccommodationZoneCommand>();
        CreateMap<CreateProductionAccommodationZoneCommand, ProductionAccommodationZone>();
        CreateMap<ProductionAccommodationZoneDto, CreateProductionAccommodationZoneResponse>();

        CreateMap<UpdateProductionAccommodationZoneRequest, UpdateProductionAccommodationZoneCommand>();
        CreateMap<UpdateProductionAccommodationZoneCommand, ProductionAccommodationZone>();
        CreateMap<ProductionAccommodationZoneDto, UpdateProductionAccommodationZoneResponse>();
    }
}