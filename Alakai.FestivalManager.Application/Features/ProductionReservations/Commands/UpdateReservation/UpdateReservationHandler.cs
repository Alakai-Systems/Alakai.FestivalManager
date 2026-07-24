namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.UpdateReservation;

public class UpdateReservationHandler
{
    private readonly IProductionReservationRepository _reservationRepository;
    private readonly IProductionPersonRepository _productionPersonRepository;
    private readonly IMapper _mapper;

    public UpdateReservationHandler(IProductionReservationRepository reservationRepository, IProductionPersonRepository productionPersonRepository, IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _productionPersonRepository = productionPersonRepository;
        _mapper = mapper;
    }

    public async Task<ReservationDto> HandleAsync(UpdateReservationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationReservation? reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException($"Reservation with id '{command.ReservationId}' was not found.");
        }

        if (command.ResponsibleProductionPersonId.HasValue)
        {
            ProductionPerson? responsible = await _productionPersonRepository.GetByIdAsync(command.ResponsibleProductionPersonId.Value, cancellationToken);

            if (responsible is null)
            {
                throw new NotFoundException($"Production person with id '{command.ResponsibleProductionPersonId.Value}' was not found.");
            }
        }

        reservation.ResponsibleProductionPersonId = command.ResponsibleProductionPersonId;
        reservation.RoomType = command.RoomType;
        reservation.Occupants.Clear();

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

        reservation.SetUpdated();

        await _reservationRepository.SaveChangesAsync(cancellationToken);

        ProductionAccommodationReservation updated = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken)
            ?? throw new NotFoundException($"Reservation with id '{command.ReservationId}' was not found after update.");

        return _mapper.Map<ReservationDto>(updated);
    }
}