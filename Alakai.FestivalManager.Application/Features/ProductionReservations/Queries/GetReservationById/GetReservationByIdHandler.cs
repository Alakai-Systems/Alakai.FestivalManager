namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Queries.GetReservationById;

public class GetReservationByIdHandler
{
    private readonly IProductionReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public GetReservationByIdHandler(IProductionReservationRepository reservationRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
    }

    public async Task<ReservationDto?> HandleAsync(GetReservationByIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationReservation? reservation = await _reservationRepository.GetByIdAsync(query.Id, cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        return _mapper.Map<ReservationDto>(reservation);
    }
}