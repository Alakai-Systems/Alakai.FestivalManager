namespace Alakai.FestivalManager.Application.Features.Users.Queries.GetUserByEmail;

public class GetUserByEmailQuery
{
    public string Email { get; set; }

    public GetUserByEmailQuery(string email)
    {
        Email = email;
    }
}
