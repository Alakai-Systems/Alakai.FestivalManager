namespace Alakai.FestivalManager.Admin.Contracts.Registrations.DTOs;

public class PublicLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool RequiresRole { get; set; }
    public bool IsFull { get; set; }
    public bool LeaderFull { get; set; }
    public bool FollowerFull { get; set; }
    public bool LeaderSoloAvailable { get; set; }
    public bool FollowerSoloAvailable { get; set; }
}
