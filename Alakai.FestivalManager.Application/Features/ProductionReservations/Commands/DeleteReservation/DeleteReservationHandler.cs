namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Commands.DeleteReservation;

public class DeleteReservationHandler
{
    private readonly IProductionReservationRepository _reservationRepository;

    public DeleteReservationHandler(IProductionReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<Guid> HandleAsync(DeleteReservationCommand command, CancellationToken cancellationToken = default)
    {
        ProductionAccommodationReservation? reservation = await _reservationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException($"Reservation with id '{command.Id}' was not found.");
        }

        _reservationRepository.Delete(reservation);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}