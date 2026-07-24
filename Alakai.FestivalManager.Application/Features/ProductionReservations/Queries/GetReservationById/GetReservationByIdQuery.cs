namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Queries.GetReservationById;

public class GetReservationByIdQuery
{
    public Guid Id { get; set; }
    public GetReservationByIdQuery(Guid id)
    {
        Id = id;
    }
}