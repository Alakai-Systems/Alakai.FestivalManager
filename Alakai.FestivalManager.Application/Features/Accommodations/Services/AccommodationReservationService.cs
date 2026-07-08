namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public class AccommodationReservationService : IAccommodationReservationService
{
    private readonly IAccommodationReservationRepository _reservationRepository;
    private readonly IAccommodationBuildingRepository _buildingRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailNotificationService _emailNotificationService;

    public AccommodationReservationService(
        IAccommodationReservationRepository reservationRepository,
        IAccommodationBuildingRepository buildingRepository,
        IRegistrationRepository registrationRepository,
        IEmailNotificationService emailNotificationService)
    {
        _reservationRepository = reservationRepository;
        _buildingRepository = buildingRepository;
        _registrationRepository = registrationRepository;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAsync(CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default)
    {
        AccommodationBuilding? building = await _buildingRepository.GetByIdAsync(command.AccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Accommodation building with id '{command.AccommodationBuildingId}' was not found.");
        }

        if (building.IsLocked)
        {
            throw new BusinessRuleException("This accommodation is currently closed for new reservations.");
        }

        Registration? responsible = await _registrationRepository.GetByIdAsync(command.ResponsibleRegistrationId, cancellationToken);

        if (responsible is null)
        {
            throw new NotFoundException($"Registration with id '{command.ResponsibleRegistrationId}' was not found.");
        }

        if (command.Occupants.Count == 0)
        {
            throw new BusinessRuleException("At least one occupant is required.");
        }

        if (building.Type == AccommodationType.Room && command.Occupants.Count > 12)
        {
            throw new BusinessRuleException("A room reservation cannot have more than 12 occupants.");
        }

        bool requiresFullDetails = building.Type != AccommodationType.Room;

        if (requiresFullDetails && command.Occupants.Any(o => o.BirthDate is null || o.DocumentExpiryDate is null))
        {
            throw new BusinessRuleException("Birth date and document expiry date are required for every occupant of this accommodation type.");
        }

        HashSet<Guid> allowedPassTypeIds = building.AllowedPassTypes.Select(p => p.PassTypeId).ToHashSet();
        List<(AccommodationOccupantInput Input, Registration Registration)> resolvedOccupants = [];

        foreach (AccommodationOccupantInput occupantInput in command.Occupants)
        {
            Registration? occupantRegistration = await _registrationRepository.GetByEditionAndEmailAsync(building.EditionId, occupantInput.Email, cancellationToken);

            if (occupantRegistration is null)
            {
                throw new BusinessRuleException($"No registration was found for '{occupantInput.Email}' in this edition.");
            }

            if (allowedPassTypeIds.Count > 0 && !allowedPassTypeIds.Contains(occupantRegistration.PassTypeId))
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' does not have a pass type allowed for this accommodation.");
            }

            if (occupantRegistration.PaymentStatus is PaymentStatus.Unpaid or PaymentStatus.Failed)
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' has not paid (fully or partially) for their registration yet.");
            }

            bool alreadyBooked = await _reservationRepository.IsRegistrationAlreadyBookedAsync(building.EditionId, occupantRegistration.Id, cancellationToken);

            if (alreadyBooked)
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' already has an accommodation reservation for this edition.");
            }

            resolvedOccupants.Add((occupantInput, occupantRegistration));
        }

        List<Accommodation> orderedUnits = building.Zones
            .OrderBy(z => z.SortOrder)
            .SelectMany(z => z.Accommodations.Where(a => a.IsActive).OrderBy(a => NaturalSortKey(a.Name)).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Dictionary<Guid, int> occupancy = await _reservationRepository.GetOccupancyCountsAsync(orderedUnits.Select(u => u.Id).ToList(), cancellationToken);

        AccommodationReservation reservation = new()
        {
            EditionId = building.EditionId,
            AccommodationBuildingId = building.Id,
            ResponsibleRegistrationId = responsible.Id
        };

        if (building.Type == AccommodationType.Room)
        {
            int remaining = resolvedOccupants.Count;
            int occupantIndex = 0;

            foreach (Accommodation unit in orderedUnits)
            {
                if (remaining <= 0)
                {
                    break;
                }

                int occupied = occupancy.GetValueOrDefault(unit.Id, 0);
                int free = unit.Capacity - occupied;

                if (free <= 0)
                {
                    continue;
                }

                int toPlace = Math.Min(free, remaining);

                for (int i = 0; i < toPlace; i++)
                {
                    (AccommodationOccupantInput input, Registration reg) = resolvedOccupants[occupantIndex];

                    reservation.Occupants.Add(new AccommodationReservationOccupant
                    {
                        Email = input.Email,
                        BirthDate = input.BirthDate,
                        DocumentExpiryDate = input.DocumentExpiryDate,
                        IsResponsible = reg.Id == responsible.Id,
                        RegistrationId = reg.Id,
                        AccommodationId = unit.Id
                    });

                    occupantIndex++;
                    remaining--;
                }
            }

            if (remaining > 0)
            {
                throw new BusinessRuleException("There is not enough available capacity for this group size right now.");
            }
        }
        else
        {
            int requestedCapacity = resolvedOccupants.Count;

            Accommodation? matchingUnit = orderedUnits.FirstOrDefault(u =>
                u.Capacity == requestedCapacity && occupancy.GetValueOrDefault(u.Id, 0) == 0);

            if (matchingUnit is null)
            {
                throw new BusinessRuleException($"There is no available {building.Type.ToString().ToLowerInvariant()} for {requestedCapacity} people right now.");
            }

            foreach ((AccommodationOccupantInput input, Registration reg) in resolvedOccupants)
            {
                reservation.Occupants.Add(new AccommodationReservationOccupant
                {
                    Email = input.Email,
                    BirthDate = input.BirthDate,
                    DocumentExpiryDate = input.DocumentExpiryDate,
                    IsResponsible = reg.Id == responsible.Id,
                    RegistrationId = reg.Id,
                    AccommodationId = matchingUnit.Id
                });
            }
        }

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        foreach (AccommodationReservationOccupant occupant in reservation.Occupants)
        {
            if (occupant.RegistrationId.HasValue)
            {
                try
                {
                    await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.AccommodationConfirmed, occupant.RegistrationId.Value, cancellationToken);
                }
                catch
                {
                    // Non-critical: the reservation is already saved even if an individual email fails to send.
                }
            }
        }

        AccommodationReservation? saved = await _reservationRepository.GetByIdAsync(reservation.Id, cancellationToken);

        return new ApiResponse<CreateAccommodationReservationResponse>
        {
            Success = true,
            Data = new CreateAccommodationReservationResponse { Reservation = ToDto(saved ?? reservation, building.Name) },
            Errors = [],
            Message = "Accommodation reservation created successfully."
        };
    }

    public async Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAsync(UpdateAccommodationReservationCommand command, bool isAdmin, CancellationToken cancellationToken = default)
    {
        AccommodationReservation? reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException($"Reservation with id '{command.ReservationId}' was not found.");
        }

        if (!isAdmin && reservation.ResponsibleRegistrationId != command.RequestingRegistrationId)
        {
            throw new BusinessRuleException("Only the person who made this reservation can modify it.");
        }

        AccommodationBuilding? building = await _buildingRepository.GetByIdAsync(reservation.AccommodationBuildingId, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException($"Accommodation building with id '{reservation.AccommodationBuildingId}' was not found.");
        }

        if (command.Occupants.Count == 0)
        {
            throw new BusinessRuleException("At least one occupant is required.");
        }

        if (building.Type == AccommodationType.Room && command.Occupants.Count > 12)
        {
            throw new BusinessRuleException("A room reservation cannot have more than 12 occupants.");
        }

        bool requiresFullDetails = building.Type != AccommodationType.Room;

        if (requiresFullDetails && command.Occupants.Any(o => o.BirthDate is null || o.DocumentExpiryDate is null))
        {
            throw new BusinessRuleException("Birth date and document expiry date are required for every occupant of this accommodation type.");
        }

        HashSet<Guid> allowedPassTypeIds = building.AllowedPassTypes.Select(p => p.PassTypeId).ToHashSet();
        List<(AccommodationOccupantInput Input, Registration Registration)> resolvedOccupants = [];

        foreach (AccommodationOccupantInput occupantInput in command.Occupants)
        {
            Registration? occupantRegistration = await _registrationRepository.GetByEditionAndEmailAsync(building.EditionId, occupantInput.Email, cancellationToken);

            if (occupantRegistration is null)
            {
                throw new BusinessRuleException($"No registration was found for '{occupantInput.Email}' in this edition.");
            }

            if (allowedPassTypeIds.Count > 0 && !allowedPassTypeIds.Contains(occupantRegistration.PassTypeId))
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' does not have a pass type allowed for this accommodation.");
            }

            if (occupantRegistration.PaymentStatus is PaymentStatus.Unpaid or PaymentStatus.Failed)
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' has not paid (fully or partially) for their registration yet.");
            }

            bool alreadyBookedElsewhere = await _reservationRepository.IsRegistrationAlreadyBookedAsync(building.EditionId, occupantRegistration.Id, cancellationToken)
                && reservation.Occupants.All(o => o.RegistrationId != occupantRegistration.Id);

            if (alreadyBookedElsewhere)
            {
                throw new BusinessRuleException($"'{occupantInput.Email}' already has a different accommodation reservation for this edition.");
            }

            resolvedOccupants.Add((occupantInput, occupantRegistration));
        }

        List<Accommodation> orderedUnits = building.Zones
            .OrderBy(z => z.SortOrder)
            .SelectMany(z => z.Accommodations.Where(a => a.IsActive).OrderBy(a => NaturalSortKey(a.Name)).ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Dictionary<Guid, int> occupancy = await _reservationRepository.GetOccupancyCountsAsync(orderedUnits.Select(u => u.Id).ToList(), cancellationToken);

        // Release this reservation's own current occupancy before recomputing, so it does not block itself.
        foreach (AccommodationReservationOccupant existing in reservation.Occupants)
        {
            if (existing.AccommodationId.HasValue && occupancy.ContainsKey(existing.AccommodationId.Value))
            {
                occupancy[existing.AccommodationId.Value] = Math.Max(0, occupancy[existing.AccommodationId.Value] - 1);
            }
        }

        reservation.Occupants.Clear();

        if (building.Type == AccommodationType.Room)
        {
            int remaining = resolvedOccupants.Count;
            int occupantIndex = 0;

            foreach (Accommodation unit in orderedUnits)
            {
                if (remaining <= 0)
                {
                    break;
                }

                int occupied = occupancy.GetValueOrDefault(unit.Id, 0);
                int free = unit.Capacity - occupied;

                if (free <= 0)
                {
                    continue;
                }

                int toPlace = Math.Min(free, remaining);

                for (int i = 0; i < toPlace; i++)
                {
                    (AccommodationOccupantInput input, Registration reg) = resolvedOccupants[occupantIndex];

                    reservation.Occupants.Add(new AccommodationReservationOccupant
                    {
                        Email = input.Email,
                        BirthDate = input.BirthDate,
                        DocumentExpiryDate = input.DocumentExpiryDate,
                        IsResponsible = reg.Id == reservation.ResponsibleRegistrationId,
                        RegistrationId = reg.Id,
                        AccommodationId = unit.Id
                    });

                    occupantIndex++;
                    remaining--;
                }
            }

            if (remaining > 0)
            {
                throw new BusinessRuleException("There is not enough available capacity for this group size right now.");
            }
        }
        else
        {
            int requestedCapacity = resolvedOccupants.Count;

            Accommodation? matchingUnit = orderedUnits.FirstOrDefault(u =>
                u.Capacity == requestedCapacity && occupancy.GetValueOrDefault(u.Id, 0) == 0);

            if (matchingUnit is null)
            {
                throw new BusinessRuleException($"There is no available {building.Type.ToString().ToLowerInvariant()} for {requestedCapacity} people right now.");
            }

            foreach ((AccommodationOccupantInput input, Registration reg) in resolvedOccupants)
            {
                reservation.Occupants.Add(new AccommodationReservationOccupant
                {
                    Email = input.Email,
                    BirthDate = input.BirthDate,
                    DocumentExpiryDate = input.DocumentExpiryDate,
                    IsResponsible = reg.Id == reservation.ResponsibleRegistrationId,
                    RegistrationId = reg.Id,
                    AccommodationId = matchingUnit.Id
                });
            }
        }

        await _reservationRepository.SaveChangesAsync(cancellationToken);

        AccommodationReservation? saved = await _reservationRepository.GetByIdAsync(reservation.Id, cancellationToken);

        return new ApiResponse<CreateAccommodationReservationResponse>
        {
            Success = true,
            Data = new CreateAccommodationReservationResponse { Reservation = ToDto(saved ?? reservation, building.Name) },
            Errors = [],
            Message = "Accommodation reservation updated successfully."
        };
    }

    public async Task<ApiResponse<GetAccommodationReservationsResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AccommodationReservation> reservations = await _reservationRepository.GetByBuildingIdAsync(buildingId, cancellationToken);

        List<AccommodationReservationDto> dtos = reservations.Select(r => ToDto(r, null)).ToList();

        return new ApiResponse<GetAccommodationReservationsResponse>
        {
            Success = true,
            Data = new GetAccommodationReservationsResponse { Reservations = dtos },
            Errors = [],
            Message = $"There are {dtos.Count} reservations."
        };
    }

    public async Task<ApiResponse<GetAccommodationReservationResponse>> GetByResponsibleRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        AccommodationReservation? reservation = await _reservationRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        return new ApiResponse<GetAccommodationReservationResponse>
        {
            Success = true,
            Data = new GetAccommodationReservationResponse { Reservation = reservation is null ? null : ToDto(reservation, reservation.AccommodationBuilding?.Name) },
            Errors = [],
            Message = reservation is null ? "No reservation found." : "Reservation found."
        };
    }

    public async Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        AccommodationReservation? reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse<DeleteAccommodationReservationResponse> { Success = true, Data = new DeleteAccommodationReservationResponse { Id = id, Deleted = false }, Errors = [], Message = "Reservation not found." };
        }

        if (!isAdmin && reservation.ResponsibleRegistrationId != requestingRegistrationId)
        {
            throw new BusinessRuleException("Only the person who made this reservation can cancel it.");
        }

        _reservationRepository.Delete(reservation);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteAccommodationReservationResponse> { Success = true, Data = new DeleteAccommodationReservationResponse { Id = id, Deleted = true }, Errors = [], Message = "Reservation cancelled successfully." };
    }

    private static int NaturalSortKey(string name)
    {
        return int.TryParse(name, out int n) ? n : int.MaxValue;
    }

    private static AccommodationReservationDto ToDto(AccommodationReservation reservation, string? buildingName)
    {
        return new AccommodationReservationDto
        {
            Id = reservation.Id,
            AccommodationBuildingId = reservation.AccommodationBuildingId,
            BuildingName = buildingName ?? reservation.AccommodationBuilding?.Name,
            ResponsibleRegistrationId = reservation.ResponsibleRegistrationId,
            ResponsibleName = reservation.ResponsibleRegistration is not null ? $"{reservation.ResponsibleRegistration.FirstName} {reservation.ResponsibleRegistration.LastName}" : null,
            Occupants = reservation.Occupants.Select(o => new AccommodationReservationOccupantDto
            {
                Id = o.Id,
                Email = o.Email,
                BirthDate = o.BirthDate,
                DocumentExpiryDate = o.DocumentExpiryDate,
                IsResponsible = o.IsResponsible,
                RegistrationId = o.RegistrationId,
                RegistrationName = o.Registration is not null ? $"{o.Registration.FirstName} {o.Registration.LastName}" : null,
                AccommodationId = o.AccommodationId,
                AccommodationName = o.Accommodation?.Name,
                ZoneName = o.Accommodation?.AccommodationZone?.Name
            }).ToList()
        };
    }
}