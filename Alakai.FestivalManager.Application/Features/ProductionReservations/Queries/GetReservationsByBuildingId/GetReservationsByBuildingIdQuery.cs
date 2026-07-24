namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Queries.GetReservationsByBuildingId;

public class GetReservationsByBuildingIdQuery
{
    public Guid ProductionAccommodationBuildingId { get; set; }
    public GetReservationsByBuildingIdQuery(Guid productionAccommodationBuildingId)
    {
        ProductionAccommodationBuildingId = productionAccommodationBuildingId;
    }
}