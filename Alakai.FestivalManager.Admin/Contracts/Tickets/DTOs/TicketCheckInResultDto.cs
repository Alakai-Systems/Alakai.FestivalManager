namespace Alakai.FestivalManager.Admin.Contracts.Tickets.DTOs;

public class TicketCheckInResultDto
{
    public Guid RegistrationId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string PassTypeName { get; set; } = string.Empty;
    public string? LevelName { get; set; }
    public bool AlreadyCheckedIn { get; set; }
    public DateTime CheckedInAt { get; set; }
}