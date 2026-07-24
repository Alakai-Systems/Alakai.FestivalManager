namespace Alakai.FestivalManager.Admin.Contracts.ProductionPeople.DTOs;

public class ProductionPersonDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public ProductionPersonCategory Category { get; set; }
    public string RoleTitle { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ManagerEmail { get; set; }
    public string? Phone { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public bool IsActive { get; set; }
}