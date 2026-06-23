namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.Requests;

public class UpdateUserPanelProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? DocumentNumber { get; set; }
    public string? DocumentCountry { get; set; }
}