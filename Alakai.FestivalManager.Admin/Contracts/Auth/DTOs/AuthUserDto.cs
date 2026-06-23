namespace Alakai.FestivalManager.Admin.Contracts.Auth.DTOs;

public class AuthUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
}
