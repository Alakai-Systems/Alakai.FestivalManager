namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Mappings;

public class TripMappingProfile : Profile
{
    public TripMappingProfile()
    {
        CreateMap<ProductionTrip, ProductionTripDto>()
            .ForMember(d => d.ProductionPersonName, opt => opt.MapFrom(s => s.ProductionPerson != null ? s.ProductionPerson.FirstName + " " + s.ProductionPerson.LastName : null));

        CreateMap<IReadOnlyList<ProductionTripDto>, IReadOnlyList<ProductionTrip>>();

        CreateMap<CreateTripRequest, CreateTripCommand>();
        CreateMap<CreateTripCommand, ProductionTrip>();
        CreateMap<ProductionTripDto, CreateTripResponse>();

        CreateMap<UpdateTripRequest, UpdateTripCommand>();
        CreateMap<UpdateTripCommand, ProductionTrip>();
        CreateMap<ProductionTripDto, UpdateTripResponse>();
    }
}