namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery
{
    public Guid Id { get; set; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}
