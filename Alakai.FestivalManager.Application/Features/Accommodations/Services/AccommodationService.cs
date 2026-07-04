using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Accommodations.Commands;
using Alakai.FestivalManager.Application.Features.Accommodations.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public class AccommodationService : IAccommodationService
{
    private readonly IAccommodationRepository _accommodationRepository;

    public AccommodationService(IAccommodationRepository accommodationRepository)
    {
        _accommodationRepository = accommodationRepository;
    }

    /// <summary>
    /// Names supports a comma-separated list (e.g. "101,102,103") to create several
    /// identical-capacity units in one call, matching how a real building's rooms
    /// are usually set up in bulk.
    /// </summary>
    public async Task<ApiResponse<CreateAccommodationResponse>> CreateAsync(CreateAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        List<string> names = command.Names
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        if (names.Count == 0)
        {
            throw new BusinessRuleException("At least one accommodation name is required.");
        }

        List<Accommodation> created = [];

        foreach (string name in names)
        {
            Accommodation accommodation = new()
            {
                AccommodationZoneId = command.AccommodationZoneId,
                Name = name,
                Capacity = command.Capacity,
                SortOrder = command.SortOrder,
                IsActive = true
            };

            await _accommodationRepository.AddAsync(accommodation, cancellationToken);
            created.Add(accommodation);
        }

        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        List<AccommodationDto> dtos = created.Select(a => new AccommodationDto
        {
            Id = a.Id,
            Name = a.Name,
            Capacity = a.Capacity,
            SortOrder = a.SortOrder,
            IsActive = a.IsActive
        }).ToList();

        return new ApiResponse<CreateAccommodationResponse> { Success = true, Data = new CreateAccommodationResponse { Accommodations = dtos }, Errors = [], Message = $"{dtos.Count} accommodation unit(s) created successfully." };
    }

    public async Task<ApiResponse<UpdateAccommodationResponse>> UpdateAsync(UpdateAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        Accommodation? accommodation = await _accommodationRepository.GetByIdAsync(command.Id, cancellationToken);

        if (accommodation is null)
        {
            throw new NotFoundException($"Accommodation with id '{command.Id}' was not found.");
        }

        accommodation.Name = command.Name;
        accommodation.Capacity = command.Capacity;
        accommodation.SortOrder = command.SortOrder;
        accommodation.IsActive = command.IsActive;

        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        AccommodationDto dto = new()
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Capacity = accommodation.Capacity,
            SortOrder = accommodation.SortOrder,
            IsActive = accommodation.IsActive
        };

        return new ApiResponse<UpdateAccommodationResponse> { Success = true, Data = new UpdateAccommodationResponse { Accommodation = dto }, Errors = [], Message = $"'{accommodation.Name}' updated successfully." };
    }

    public async Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Accommodation? accommodation = await _accommodationRepository.GetByIdAsync(id, cancellationToken);

        if (accommodation is null)
        {
            return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = false }, Errors = [], Message = "Accommodation not found." };
        }

        _accommodationRepository.Delete(accommodation);
        await _accommodationRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteAccommodationEntityResponse> { Success = true, Data = new DeleteAccommodationEntityResponse { Id = id, Deleted = true }, Errors = [], Message = "Accommodation deleted successfully." };
    }
}