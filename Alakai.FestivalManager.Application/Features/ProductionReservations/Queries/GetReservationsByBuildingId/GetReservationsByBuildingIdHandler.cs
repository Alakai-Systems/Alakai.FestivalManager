namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Queries.GetReservationsByBuildingId;

public class GetReservationsByBuildingIdHandler
{
    private readonly IProductionReservationRepository _reservationRepository;
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetReservationsByBuildingIdHandler(IProductionReservationRepository reservationRepository, IProductionAccommodationBuildingRepository buildingRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReservationDto>> HandleAsync(GetReservationsByBuildingIdQuery query, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(query.ProductionAccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{query.ProductionAccommodationBuildingId}' was not found.");
        }

        IReadOnlyList<ProductionAccommodationReservation> reservations = await _reservationRepository.GetByBuildingIdAsync(query.ProductionAccommodationBuildingId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ReservationDto>>(reservations);
    }
}