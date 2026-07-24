namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Contracts.Responses;

public class DeleteReservationResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}