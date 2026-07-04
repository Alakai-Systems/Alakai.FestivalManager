using Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;

namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;

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