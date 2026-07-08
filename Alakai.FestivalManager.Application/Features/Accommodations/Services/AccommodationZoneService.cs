namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public class AccommodationZoneService : IAccommodationZoneService
{
    private readonly IAccommodationZoneRepository _zoneRepository;

    public AccommodationZoneService(IAccommodationZoneRepository zoneRepository)
    {
        _zoneRepository = zoneRepository;
    }

    public async Task<ApiResponse<CreateAccommodationZoneResponse>> CreateAsync(CreateAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        AccommodationZone zone = new()
        {
            AccommodationBuildingId = command.AccommodationBuildingId,
            Name = command.Name,
            SortOrder = command.SortOrder
        };

        await _zoneRepository.AddAsync(zone, cancellationToken);
        await _zoneRepository.SaveChangesAsync(cancellationToken);

        AccommodationZoneDto dto = new() { Id = zone.Id, Name = zone.Name, SortOrder = zone.SortOrder };

        return new ApiResponse<CreateAccommodationZoneResponse> { Success = true, Data = new CreateAccommodationZoneResponse { Zone = dto }, Errors = [], Message = $"Zone '{zone.Name}' created successfully." };
    }

    public async Task<ApiResponse<UpdateAccommodationZoneResponse>> UpdateAsync(UpdateAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        AccommodationZone? zone = await _zoneRepository.GetByIdAsync(command.Id, cancellationToken);

        if (zone is null)
        {
            throw new NotFoundException($"Accommodation zone with id '{command.Id}' was not found.");
        }

        zone.Name = command.Name;
        zone.SortOrder = command.SortOrder;

        await _zoneRepository.SaveChangesAsync(cancellationToken);

        AccommodationZoneDto dto = new() { Id = zone.Id, Name = zone.Name, SortOrder = zone.SortOrder };

        return new ApiResponse<UpdateAccommodationZoneResponse> { Success = true, Data = new UpdateAccommodationZoneResponse { Zone = dto }, Errors = [], Message = $"Zone '{zone.Name}' updated successfully." };
    }

    public async Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        AccommodationZone? zone = await _zoneRepository.GetByIdAsync(id, cancellationToken);

        if (zone is null)
        {
            return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = false }, Errors = [], Message = "Zone not found." };
        }

        _zoneRepository.Delete(zone);
        await _zoneRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = true }, Errors = [], Message = "Zone deleted successfully." };
    }
}