namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.CreateReservation;

public class CreateReservationHandler
{
    private readonly IProductionReservationRepository _reservationRepository;
    private readonly IProductionAccommodationBuildingRepository _buildingRepository;
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public CreateReservationHandler(IProductionReservationRepository reservationRepository, IProductionAccommodationBuildingRepository buildingRepository, IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<ReservationDto> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.ProductionAccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Production accommodation building with id '{command.ProductionAccommodationBuildingId}' was not found.");
        }

        if (building.IsLocked)
        {
            throw new BusinessRuleException("This accommodation is currently closed for new reservations.");
        }

        if (command.ResponsibleProductionPersonId.HasValue)
        {
            ProductionPerson? responsible = await _productionPersonRepository.GetByIdAsync(command.ResponsibleProductionPersonId.Value, cancellationToken);

            if (responsible is null)
            {
                throw new NotFoundException($"Production person with id '{command.ResponsibleProductionPersonId.Value}' was not found.");
            }
        }

        ProductionAccommodationReservation reservation = new()
        {
            EditionId = command.EditionId,
            ProductionAccommodationBuildingId = command.ProductionAccommodationBuildingId,
            ResponsibleProductionPersonId = command.ResponsibleProductionPersonId,
            RoomType = command.RoomType
        };

        foreach (ReservationOccupantInput occupantInput in command.Occupants)
        {
            ProductionPerson? person = await _productionPersonRepository.GetByIdAsync(occupantInput.ProductionPersonId, cancellationToken);

            if (person is null)
            {
                throw new NotFoundException($"Production person with id '{occupantInput.ProductionPersonId}' was not found.");
            }

            reservation.Occupants.Add(new ProductionAccommodationReservationOccupant
            {
                ProductionPersonId = occupantInput.ProductionPersonId,
                ProductionAccommodationId = occupantInput.ProductionAccommodationId,
                IsResponsible = command.ResponsibleProductionPersonId.HasValue && occupantInput.ProductionPersonId == command.ResponsibleProductionPersonId.Value
            });
        }

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        ProductionAccommodationReservation created = await _reservationRepository.GetByIdAsync(reservation.Id, cancellationToken)
            ?? throw new NotFoundException($"Reservation with id '{reservation.Id}' was not found after creation.");

        return _mapper.Map<ReservationDto>(created);
    }
}