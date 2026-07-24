namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Mappings;

public class ProductionPersonMappingProfile : Profile
{
    public ProductionPersonMappingProfile()
    {
        //Generics and Get
        CreateMap<ProductionPerson, ProductionPersonDto>();
        CreateMap<IReadOnlyList<ProductionPersonDto>, IReadOnlyList<ProductionPerson>>();

        //Create ProductionPerson
        CreateMap<CreateProductionPersonRequest, CreateProductionPersonCommand>();
        CreateMap<CreateProductionPersonCommand, ProductionPerson>();
        CreateMap<ProductionPersonDto, CreateProductionPersonResponse>();

        //Update ProductionPerson
        CreateMap<UpdateProductionPersonRequest, UpdateProductionPersonCommand>();
        CreateMap<UpdateProductionPersonCommand, ProductionPerson>();
        CreateMap<ProductionPersonDto, UpdateProductionPersonResponse>();
    }
}