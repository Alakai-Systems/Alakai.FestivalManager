namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Commands.DeleteTrip;

public class DeleteTripCommand
{
    public Guid Id { get; set; }
    public DeleteTripCommand(Guid id)
    {
        Id = id;
    }
}