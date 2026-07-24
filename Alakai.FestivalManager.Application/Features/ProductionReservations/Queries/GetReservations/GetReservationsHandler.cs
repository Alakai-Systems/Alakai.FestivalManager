namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Queries.GetReservations;

public class GetReservationsHandler
{
    private readonly IProductionReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public GetReservationsHandler(IProductionReservationRepository reservationRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReservationDto>> HandleAsync(GetReservationsQuery query, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ProductionAccommodationReservation> reservations = await _reservationRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ReservationDto>>(reservations);
    }
}