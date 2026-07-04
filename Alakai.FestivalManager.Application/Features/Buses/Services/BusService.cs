using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Buses.Commands;
using Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Buses.Services;

public class BusService : IBusService
{
    private readonly IBusRepository _busRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IBusReservationRepository _reservationRepository;

    public BusService(IBusRepository busRepository, IRegistrationRepository registrationRepository, IBusReservationRepository reservationRepository)
    {
        _busRepository = busRepository;
        _registrationRepository = registrationRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<ApiResponse<GetBusesResponse>> GetAllAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Bus> buses = await _busRepository.GetByEditionIdAsync(editionId, cancellationToken);

        List<BusDto> dtos = buses.Select(b => ToDto(b, 0)).ToList();

        return new ApiResponse<GetBusesResponse> { Success = true, Data = new GetBusesResponse { Buses = dtos }, Errors = [], Message = $"There are {dtos.Count} buses." };
    }

    public async Task<ApiResponse<GetBusesResponse>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{registrationId}' was not found.");
        }

        IReadOnlyList<Bus> buses = await _busRepository.GetByEditionIdAsync(registration.EditionId, cancellationToken);

        List<Bus> eligible = buses
            .Where(b => b.IsActive)
            .Where(b => b.AllowedPassTypes.Count == 0 || b.AllowedPassTypes.Any(p => p.PassTypeId == registration.PassTypeId))
            .ToList();

        List<BusDto> dtos = [];

        foreach (Bus bus in eligible)
        {
            int occupied = await _reservationRepository.GetOccupiedCountAsync(bus.Id, cancellationToken);
            dtos.Add(ToDto(bus, occupied));
        }

        return new ApiResponse<GetBusesResponse> { Success = true, Data = new GetBusesResponse { Buses = dtos }, Errors = [], Message = $"There are {dtos.Count} buses available." };
    }

    public async Task<ApiResponse<GetBusResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Bus? bus = await _busRepository.GetByIdAsync(id, cancellationToken);

        if (bus is null)
        {
            throw new NotFoundException($"Bus with id '{id}' was not found.");
        }

        return new ApiResponse<GetBusResponse> { Success = true, Data = new GetBusResponse { Bus = ToDto(bus, 0) }, Errors = [], Message = "Bus loaded." };
    }

    public async Task<ApiResponse<CreateBusResponse>> CreateAsync(CreateBusCommand command, CancellationToken cancellationToken = default)
    {
        Bus bus = new()
        {
            EditionId = command.EditionId,
            Direction = command.Direction,
            DepartureTime = command.DepartureTime,
            PickupLocation = command.PickupLocation,
            DestinationLocation = command.DestinationLocation,
            Capacity = command.Capacity,
            Price = command.Price,
            IsActive = true
        };

        foreach (Guid passTypeId in command.AllowedPassTypeIds.Distinct())
        {
            bus.AllowedPassTypes.Add(new BusPassType { PassTypeId = passTypeId });
        }

        await _busRepository.AddAsync(bus, cancellationToken);
        await _busRepository.SaveChangesAsync(cancellationToken);

        Bus? saved = await _busRepository.GetByIdAsync(bus.Id, cancellationToken);

        return new ApiResponse<CreateBusResponse> { Success = true, Data = new CreateBusResponse { Bus = ToDto(saved ?? bus, 0) }, Errors = [], Message = "Bus created successfully." };
    }

    public async Task<ApiResponse<UpdateBusResponse>> UpdateAsync(UpdateBusCommand command, CancellationToken cancellationToken = default)
    {
        Bus? bus = await _busRepository.GetByIdAsync(command.Id, cancellationToken);

        if (bus is null)
        {
            throw new NotFoundException($"Bus with id '{command.Id}' was not found.");
        }

        bus.Direction = command.Direction;
        bus.DepartureTime = command.DepartureTime;
        bus.PickupLocation = command.PickupLocation;
        bus.DestinationLocation = command.DestinationLocation;
        bus.Capacity = command.Capacity;
        bus.Price = command.Price;
        bus.IsActive = command.IsActive;

        List<Guid> desiredPassTypeIds = command.AllowedPassTypeIds.Distinct().ToList();
        List<Guid> currentPassTypeIds = bus.AllowedPassTypes.Select(p => p.PassTypeId).ToList();

        List<BusPassType> toRemove = bus.AllowedPassTypes.Where(p => !desiredPassTypeIds.Contains(p.PassTypeId)).ToList();

        foreach (BusPassType entry in toRemove)
        {
            bus.AllowedPassTypes.Remove(entry);
        }

        foreach (Guid passTypeId in desiredPassTypeIds.Except(currentPassTypeIds))
        {
            bus.AllowedPassTypes.Add(new BusPassType { BusId = bus.Id, PassTypeId = passTypeId });
        }

        await _busRepository.SaveChangesAsync(cancellationToken);

        Bus? refreshed = await _busRepository.GetByIdAsync(bus.Id, cancellationToken);

        return new ApiResponse<UpdateBusResponse> { Success = true, Data = new UpdateBusResponse { Bus = ToDto(refreshed ?? bus, 0) }, Errors = [], Message = "Bus updated successfully." };
    }

    public async Task<ApiResponse<DeleteBusResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Bus? bus = await _busRepository.GetByIdAsync(id, cancellationToken);

        if (bus is null)
        {
            return new ApiResponse<DeleteBusResponse> { Success = true, Data = new DeleteBusResponse { Id = id, Deleted = false }, Errors = [], Message = "Bus not found." };
        }

        _busRepository.Delete(bus);
        await _busRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteBusResponse> { Success = true, Data = new DeleteBusResponse { Id = id, Deleted = true }, Errors = [], Message = "Bus deleted successfully." };
    }

    private static BusDto ToDto(Bus bus, int occupiedCount)
    {
        return new BusDto
        {
            Id = bus.Id,
            EditionId = bus.EditionId,
            Direction = bus.Direction,
            DepartureTime = bus.DepartureTime,
            PickupLocation = bus.PickupLocation,
            DestinationLocation = bus.DestinationLocation,
            Capacity = bus.Capacity,
            OccupiedCount = occupiedCount,
            Price = bus.Price,
            IsActive = bus.IsActive,
            AllowedPassTypeIds = bus.AllowedPassTypes.Select(p => p.PassTypeId).ToList(),
            AllowedPassTypeNames = bus.AllowedPassTypes.Select(p => p.PassType?.Name ?? string.Empty).Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
        };
    }
}