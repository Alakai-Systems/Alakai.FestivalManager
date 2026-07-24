namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Contracts.Responses;

public class DeleteTripResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}