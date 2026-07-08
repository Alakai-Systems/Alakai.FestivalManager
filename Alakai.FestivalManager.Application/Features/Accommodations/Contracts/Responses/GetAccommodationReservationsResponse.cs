namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;

public class GetAccommodationReservationsResponse
{
    public List<AccommodationReservationDto> Reservations { get; set; } = [];
}
