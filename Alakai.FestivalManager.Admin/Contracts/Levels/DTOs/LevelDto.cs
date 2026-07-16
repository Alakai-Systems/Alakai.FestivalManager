namespace Alakai.FestivalManager.Admin.Contracts.Levels.DTOs;

public class LevelDto
{
    public Guid Id { get; set; }
    public Guid PassTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal EarlyBirdPrice { get; set; }
    public decimal GroupPrice { get; set; }
    public decimal RegularPrice { get; set; }
    public int? LeaderCapacity { get; set; }
    public int? FollowerCapacity { get; set; }
    public int? IndividualCapacity { get; set; }
    public int? MaxLeaderDifference { get; set; }
    public int? MaxFollowerDifference { get; set; }
    public int CurrentLeaders { get; set; }
    public int CurrentFollowers { get; set; }
    public int CurrentIndividuals { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public string OccupationText
    {
        get
        {
            List<string> values = [];

            if (LeaderCapacity > 0)
            {
                values.Add($"L {CurrentLeaders}/{LeaderCapacity}");
            }

            if (FollowerCapacity > 0)
            {
                values.Add($"F {CurrentFollowers}/{FollowerCapacity}");
            }

            if (IndividualCapacity > 0)
            {
                values.Add($"I {CurrentIndividuals}/{IndividualCapacity}");
            }

            return string.Join(" | ", values);
        }
    }
}


