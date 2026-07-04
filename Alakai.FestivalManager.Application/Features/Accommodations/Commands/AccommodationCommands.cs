namespace Alakai.FestivalManager.Application.Features.Accommodations.Commands;

public class CreateAccommodationBuildingCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class UpdateAccommodationBuildingCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class CreateAccommodationZoneCommand
{
    public Guid AccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateAccommodationZoneCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CreateAccommodationCommand
{
    public Guid AccommodationZoneId { get; set; }
    public string Names { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateAccommodationCommand
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}