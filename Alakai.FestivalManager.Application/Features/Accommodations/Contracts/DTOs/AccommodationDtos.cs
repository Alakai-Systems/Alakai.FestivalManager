namespace Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;

public class AccommodationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int OccupiedCount { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class AccommodationZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<AccommodationDto> Accommodations { get; set; } = [];
}

public class AccommodationBuildingDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
    public List<string> AllowedPassTypeNames { get; set; } = [];
    public List<AccommodationZoneDto> Zones { get; set; } = [];
}

public class AccommodationBuildingSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccommodationType Type { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
    public int TotalCapacity { get; set; }
    public int ZoneCount { get; set; }
    public int UnitCount { get; set; }
}