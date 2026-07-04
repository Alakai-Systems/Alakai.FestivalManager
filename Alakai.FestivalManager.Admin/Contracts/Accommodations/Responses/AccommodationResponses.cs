using Alakai.FestivalManager.Admin.Contracts.Accommodations.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Accommodations.Responses;

public class GetAccommodationBuildingsResponse
{
    public List<AccommodationBuildingSummaryDto> Buildings { get; set; } = [];
}

public class GetAccommodationBuildingResponse
{
    public AccommodationBuildingDto Building { get; set; } = default!;
}

public class CreateAccommodationBuildingResponse
{
    public AccommodationBuildingDto Building { get; set; } = default!;
}

public class UpdateAccommodationBuildingResponse
{
    public AccommodationBuildingDto Building { get; set; } = default!;
}

public class CreateAccommodationZoneResponse
{
    public AccommodationZoneDto Zone { get; set; } = default!;
}

public class UpdateAccommodationZoneResponse
{
    public AccommodationZoneDto Zone { get; set; } = default!;
}

public class CreateAccommodationResponse
{
    public List<AccommodationDto> Accommodations { get; set; } = [];
}

public class UpdateAccommodationResponse
{
    public AccommodationDto Accommodation { get; set; } = default!;
}

public class DeleteAccommodationEntityResponse
{
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
}