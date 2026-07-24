namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Queries.GetProductionPersonById;

public class GetProductionPersonByIdQuery
{
    public Guid Id { get; set; }
    public GetProductionPersonByIdQuery(Guid id)
    {
        Id = id;
    }
}