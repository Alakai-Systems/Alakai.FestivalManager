using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsActive { get; set; }
    public UserRole Role { get; set; }
}