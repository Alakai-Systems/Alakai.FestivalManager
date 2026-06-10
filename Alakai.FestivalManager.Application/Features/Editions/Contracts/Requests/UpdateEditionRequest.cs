namespace Alakai.FestivalManager.Application.Features.Editions.Contracts.Requests;

public class UpdateEditionRequest
{
    public Guid FestivalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RegistrationOpenDate { get; set; }
    public DateTime? RegistrationCloseDate { get; set; }
    public bool IsActive { get; set; }
}
