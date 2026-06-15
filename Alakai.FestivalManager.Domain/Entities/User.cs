namespace Alakai.FestivalManager.Domain.Entities;

public class User : BaseEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Registration> Registrations { get; set; } = [];
}
