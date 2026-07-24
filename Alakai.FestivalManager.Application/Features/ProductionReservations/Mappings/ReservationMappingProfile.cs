namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Mappings;

public class ReservationMappingProfile : Profile
{
    public ReservationMappingProfile()
    {
        CreateMap<ProductionAccommodationReservation, ReservationDto>()
            .ForMember(d => d.BuildingName, opt => opt.MapFrom(s => s.ProductionAccommodationBuilding != null ? s.ProductionAccommodationBuilding.Name : null))
            .ForMember(d => d.ResponsibleName, opt => opt.MapFrom(s => s.ResponsibleProductionPerson != null ? s.ResponsibleProductionPerson.FirstName + " " + s.ResponsibleProductionPerson.LastName : null));

        CreateMap<IReadOnlyList<ReservationDto>, IReadOnlyList<ProductionAccommodationReservation>>();

        CreateMap<ProductionAccommodationReservationOccupant, ReservationOccupantDto>()
            .ForMember(d => d.ProductionPersonName, opt => opt.MapFrom(s => s.ProductionPerson != null ? s.ProductionPerson.FirstName + " " + s.ProductionPerson.LastName : null))
            .ForMember(d => d.AccommodationName, opt => opt.MapFrom(s => s.ProductionAccommodation != null ? s.ProductionAccommodation.Name : null))
            .ForMember(d => d.ZoneName, opt => opt.MapFrom(s => s.ProductionAccommodation != null && s.ProductionAccommodation.ProductionAccommodationZone != null ? s.ProductionAccommodation.ProductionAccommodationZone.Name : null));

        CreateMap<CreateReservationRequest, CreateReservationCommand>();
        CreateMap<UpdateReservationRequest, UpdateReservationCommand>();
        CreateMap<ReservationDto, CreateReservationResponse>();
        CreateMap<ReservationDto, UpdateReservationResponse>();
    }
}