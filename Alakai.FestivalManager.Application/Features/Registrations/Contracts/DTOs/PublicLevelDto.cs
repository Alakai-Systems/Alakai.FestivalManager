namespace Alakai.FestivalManager.Application.Features.Registrations.Contracts.DTOs;

public class PublicLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }

    /// <summary>True when the level defines Leader/Follower capacities (role must be chosen).</summary>
    public bool RequiresRole { get; set; }

    public bool IsFull { get; set; }
    public bool LeaderFull { get; set; }
    public bool FollowerFull { get; set; }

    /// <summary>False when signing up WITHOUT a partner for this role would exceed the allowed leader/follower imbalance.</summary>
    public bool LeaderSoloAvailable { get; set; }
    public bool FollowerSoloAvailable { get; set; }
}
