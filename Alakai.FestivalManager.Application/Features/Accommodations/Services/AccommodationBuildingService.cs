namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public class AccommodationBuildingService : IAccommodationBuildingService
{
    private readonly IAccommodationBuildingRepository _buildingRepository;
    private readonly IAccommodationReservationRepository _reservationRepository;
    private readonly IRegistrationRepository _registrationRepository;

    public AccommodationBuildingService(
        IAccommodationBuildingRepository buildingRepository,
        IAccommodationReservationRepository reservationRepository,
        IRegistrationRepository registrationRepository)
    {
        _buildingRepository = buildingRepository;
        _reservationRepository = reservationRepository;
        _registrationRepository = registrationRepository;
    }

    public async Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAllAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccommodationBuilding> buildings = await _buildingRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<AccommodationBuildingSummaryDto> dtos = buildings.Select(b => new AccommodationBuildingSummaryDto
        {
            Id = b.Id,
            Name = b.Name,
            Type = b.Type,
            IsLocked = b.IsLocked,
            IsActive = b.IsActive,
            TotalCapacity = b.Zones.SelectMany(z => z.Accommodations).Sum(a => a.Capacity),
            ZoneCount = b.Zones.Count,
            UnitCount = b.Zones.SelectMany(z => z.Accommodations).Count()
        }).ToList();

        return new ApiResponse<GetAccommodationBuildingsResponse> { Success = true, Data = new GetAccommodationBuildingsResponse { Buildings = dtos }, Errors = [], Message = $"There are {dtos.Count} buildings." };
    }

    public async Task<ApiResponse<GetAccommodationBuildingResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        AccommodationBuilding? building = await _buildingRepository.GetByIdAsync(id, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Accommodation building with id '{id}' was not found.");
        }

        List<Guid> unitIds = building.Zones.SelectMany(z => z.Accommodations).Select(a => a.Id).ToList();
        Dictionary<Guid, int> occupancy = await _reservationRepository.GetOccupancyCountsAsync(unitIds, cancellationToken);

        return new ApiResponse<GetAccommodationBuildingResponse> { Success = true, Data = new GetAccommodationBuildingResponse { Building = ToDto(building, occupancy) }, Errors = [], Message = "Building loaded." };
    }

    public async Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{registrationId}' was not found.");
        }

        IReadOnlyList<AccommodationBuilding> buildings = await _buildingRepository.GetByEditionIdAsync(registration.EditionId, cancellationToken);

        List<AccommodationBuildingSummaryDto> dtos = buildings
            .Where(b => b.IsActive && !b.IsLocked)
            .Where(b => b.AllowedPassTypes.Count == 0 || b.AllowedPassTypes.Any(p => p.PassTypeId == registration.PassTypeId))
            .Select(b => new AccommodationBuildingSummaryDto
            {
                Id = b.Id,
                Name = b.Name,
                Type = b.Type,
                IsLocked = b.IsLocked,
                IsActive = b.IsActive,
                TotalCapacity = b.Zones.SelectMany(z => z.Accommodations).Sum(a => a.Capacity),
                ZoneCount = b.Zones.Count,
                UnitCount = b.Zones.SelectMany(z => z.Accommodations).Count()
            }).ToList();

        return new ApiResponse<GetAccommodationBuildingsResponse> { Success = true, Data = new GetAccommodationBuildingsResponse { Buildings = dtos }, Errors = [], Message = $"There are {dtos.Count} buildings available." };
    }

    public async Task<ApiResponse<CreateAccommodationBuildingResponse>> CreateAsync(CreateAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        AccommodationBuilding building = new()
        {
            EditionId = command.EditionId,
            Name = command.Name,
            Type = command.Type,
            IsLocked = command.IsLocked,
            SortOrder = command.SortOrder,
            IsActive = true
        };

        foreach (Guid passTypeId in command.AllowedPassTypeIds.Distinct())
        {
            building.AllowedPassTypes.Add(new AccommodationBuildingPassType { PassTypeId = passTypeId });
        }

        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        AccommodationBuilding? saved = await _buildingRepository.GetByIdAsync(building.Id, cancellationToken);

        return new ApiResponse<CreateAccommodationBuildingResponse> { Success = true, Data = new CreateAccommodationBuildingResponse { Building = ToDto(saved ?? building) }, Errors = [], Message = $"Building '{building.Name}' created successfully." };
    }

    public async Task<ApiResponse<UpdateAccommodationBuildingResponse>> UpdateAsync(UpdateAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        AccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.Id, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Accommodation building with id '{command.Id}' was not found.");
        }

        building.Name = command.Name;
        building.Type = command.Type;
        building.IsLocked = command.IsLocked;
        building.SortOrder = command.SortOrder;
        building.IsActive = command.IsActive;

        List<Guid> desiredPassTypeIds = command.AllowedPassTypeIds.Distinct().ToList();
        List<Guid> currentPassTypeIds = building.AllowedPassTypes.Select(p => p.PassTypeId).ToList();

        List<AccommodationBuildingPassType> toRemove = building.AllowedPassTypes
            .Where(p => !desiredPassTypeIds.Contains(p.PassTypeId))
            .ToList();

        foreach (AccommodationBuildingPassType entry in toRemove)
        {
            _buildingRepository.RemovePassType(entry);
        }

        foreach (Guid passTypeId in desiredPassTypeIds.Except(currentPassTypeIds))
        {
            _buildingRepository.AddPassType(new AccommodationBuildingPassType
            {
                AccommodationBuildingId = building.Id,
                PassTypeId = passTypeId
            });
        }

        await _buildingRepository.SaveChangesAsync(cancellationToken);

        AccommodationBuilding? refreshed = await _buildingRepository.GetByIdAsync(building.Id, cancellationToken);

        return new ApiResponse<UpdateAccommodationBuildingResponse> { Success = true, Data = new UpdateAccommodationBuildingResponse { Building = ToDto(refreshed ?? building) }, Errors = [], Message = $"Building '{building.Name}' updated successfully." };
    }

    public async Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        AccommodationBuilding? building = await _buildingRepository.GetByIdAsync(id, cancellationToken);

        if (building is null)
        {
            return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = false }, Errors = [], Message = "Building not found." };
        }

        _buildingRepository.Delete(building);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = true }, Errors = [], Message = "Building deleted successfully." };
    }

    private static AccommodationBuildingDto ToDto(AccommodationBuilding building, Dictionary<Guid, int>? occupancy = null)
    {
        occupancy ??= [];

        return new AccommodationBuildingDto
        {
            Id = building.Id,
            EditionId = building.EditionId,
            Name = building.Name,
            Type = building.Type,
            IsLocked = building.IsLocked,
            SortOrder = building.SortOrder,
            IsActive = building.IsActive,
            AllowedPassTypeIds = building.AllowedPassTypes.Select(p => p.PassTypeId).ToList(),
            AllowedPassTypeNames = building.AllowedPassTypes.Select(p => p.PassType?.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList(),
            Zones = building.Zones.Select(z => new AccommodationZoneDto
            {
                Id = z.Id,
                Name = z.Name,
                SortOrder = z.SortOrder,
                Accommodations = z.Accommodations.Select(a => new AccommodationDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Capacity = a.Capacity,
                    OccupiedCount = occupancy.GetValueOrDefault(a.Id, 0),
                    SortOrder = a.SortOrder,
                    IsActive = a.IsActive
                }).ToList()
            }).ToList()
        };
    }
}
