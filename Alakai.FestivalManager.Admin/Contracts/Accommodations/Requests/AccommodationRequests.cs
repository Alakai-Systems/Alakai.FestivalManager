namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Requests;

public class CreateAccommodationBuildingRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; } = 1;
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class UpdateAccommodationBuildingRequest
{
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public bool IsLocked { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<Guid> AllowedPassTypeIds { get; set; } = [];
}

public class CreateAccommodationZoneRequest
{
    public Guid AccommodationBuildingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class UpdateAccommodationZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CreateAccommodationRequest
{
    public Guid AccommodationZoneId { get; set; }
    public string Names { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateAccommodationRequest
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}