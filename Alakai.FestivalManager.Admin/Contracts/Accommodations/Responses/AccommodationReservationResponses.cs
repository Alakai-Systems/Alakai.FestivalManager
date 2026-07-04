using Alakai.FestivalManager.Admin.Contracts.Accommodations.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Responses;

public class CreateAccommodationReservationResponse
{
    public AccommodationReservationDto Reservation { get; set; } = default!;
}

public class GetAccommodationReservationResponse
{
    public AccommodationReservationDto? Reservation { get; set; }
}

public class GetAccommodationReservationsResponse
{
    public List<AccommodationReservationDto> Reservations { get; set; } = [];
}

public class DeleteAccommodationReservationResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}