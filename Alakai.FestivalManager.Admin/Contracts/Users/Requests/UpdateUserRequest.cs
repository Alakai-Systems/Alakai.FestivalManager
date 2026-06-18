namespace Alakai.FestivalManager.Admin.Contracts.Users.Requests;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsActive { get; set; }
}
