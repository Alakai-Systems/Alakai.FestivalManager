namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Mappings;

public class ProductionSupplierMappingProfile : Profile
{
    public ProductionSupplierMappingProfile()
    {
        CreateMap<ProductionSupplier, ProductionSupplierDto>();
        CreateMap<IReadOnlyList<ProductionSupplierDto>, IReadOnlyList<ProductionSupplier>>();

        CreateMap<CreateProductionSupplierRequest, CreateProductionSupplierCommand>();
        CreateMap<CreateProductionSupplierCommand, ProductionSupplier>();
        CreateMap<ProductionSupplierDto, CreateProductionSupplierResponse>();

        CreateMap<UpdateProductionSupplierRequest, UpdateProductionSupplierCommand>();
        CreateMap<UpdateProductionSupplierCommand, ProductionSupplier>();
        CreateMap<ProductionSupplierDto, UpdateProductionSupplierResponse>();
    }
}