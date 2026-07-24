namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Mappings;

public class ProductionAccommodationMappingProfile : Profile
{
    public ProductionAccommodationMappingProfile()
    {
        CreateMap<ProductionAccommodation, ProductionAccommodationDto>();
        CreateMap<IReadOnlyList<ProductionAccommodationDto>, IReadOnlyList<ProductionAccommodation>>();

        CreateMap<CreateProductionAccommodationRequest, CreateProductionAccommodationCommand>();
        CreateMap<CreateProductionAccommodationCommand, ProductionAccommodation>();
        CreateMap<ProductionAccommodationDto, CreateProductionAccommodationResponse>();

        CreateMap<UpdateProductionAccommodationRequest, UpdateProductionAccommodationCommand>();
        CreateMap<UpdateProductionAccommodationCommand, ProductionAccommodation>();
        CreateMap<ProductionAccommodationDto, UpdateProductionAccommodationResponse>();
    }
}