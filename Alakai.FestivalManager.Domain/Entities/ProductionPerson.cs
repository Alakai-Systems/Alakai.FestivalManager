namespace Alakai.FestivalManager.Domain.Entities;

public class ProductionPerson : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public ProductionPersonCategory Category { get; set; }
    public string RoleTitle { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Nationality { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProductionTrip> Trips { get; set; } = new List<ProductionTrip>();
    public ICollection<ProductionAccommodationReservationOccupant> AccommodationOccupancies { get; set; } = new List<ProductionAccommodationReservationOccupant>();
}