using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Common.Exceptions;
using Alakai.FestivalManager.Application.Features.Buses.Commands;
using Alakai.FestivalManager.Application.Features.Buses.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Buses.Contracts.Responses;
using Alakai.FestivalManager.Application.Features.Emails.Services;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;
using Alakai.FestivalManager.Domain.Enums;

namespace Alakai.FestivalManager.Application.Features.Buses.Services;

public class BusReservationService : IBusReservationService
{
    private readonly IBusReservationRepository _reservationRepository;
    private readonly IBusRepository _busRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailNotificationService _emailNotificationService;

    public BusReservationService(
        IBusReservationRepository reservationRepository,
        IBusRepository busRepository,
        IRegistrationRepository registrationRepository,
        IEmailNotificationService emailNotificationService)
    {
        _reservationRepository = reservationRepository;
        _busRepository = busRepository;
        _registrationRepository = registrationRepository;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BusReservation> reservations = await _reservationRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        return new ApiResponse<GetBusReservationsResponse>
        {
            Success = true,
            Data = new GetBusReservationsResponse { Reservations = reservations.Select(r => ToDto(r)).ToList() },
            Errors = [],
            Message = $"There are {reservations.Count} reservations."
        };
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> GetByBusIdAsync(Guid busId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BusReservation> reservations = await _reservationRepository.GetByBusIdAsync(busId, cancellationToken);

        return new ApiResponse<GetBusReservationsResponse>
        {
            Success = true,
            Data = new GetBusReservationsResponse { Reservations = reservations.Select(r => ToDto(r)).ToList() },
            Errors = [],
            Message = $"There are {reservations.Count} reservations."
        };
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<BusReservation> reservations = await _reservationRepository.GetByEditionIdAsync(editionId, cancellationToken);

        return new ApiResponse<GetBusReservationsResponse>
        {
            Success = true,
            Data = new GetBusReservationsResponse { Reservations = reservations.Select(r => ToDto(r)).ToList() },
            Errors = [],
            Message = $"There are {reservations.Count} reservations."
        };
    }

    public async Task<ApiResponse<CreateBusReservationResponse>> CreateAsync(CreateBusReservationCommand command, CancellationToken cancellationToken = default)
    {
        BusReservation reservation = await CreateInternalAsync(command.BusId, command.RegistrationId, excludeReservationId: null, sendEmail: true, cancellationToken);

        return new ApiResponse<CreateBusReservationResponse>
        {
            Success = true,
            Data = new CreateBusReservationResponse { Reservation = ToDto(reservation) },
            Errors = [],
            Message = "Bus reservation created successfully."
        };
    }

    public async Task<ApiResponse<GetBusReservationsResponse>> CreateManyAsync(CreateBusReservationsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.BusIds.Count == 0)
        {
            throw new BusinessRuleException("Select at least one bus.");
        }

        List<BusReservation> created = [];

        foreach (Guid busId in command.BusIds.Distinct())
        {
            BusReservation reservation = await CreateInternalAsync(busId, command.RegistrationId, excludeReservationId: null, sendEmail: false, cancellationToken);
            created.Add(reservation);
        }

        try
        {
            await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.BusConfirmed, command.RegistrationId, cancellationToken);
        }
        catch
        {
            // Non-critical: the reservations are already saved even if the email fails to send.
        }

        return new ApiResponse<GetBusReservationsResponse>
        {
            Success = true,
            Data = new GetBusReservationsResponse { Reservations = created.Select(r => ToDto(r)).ToList() },
            Errors = [],
            Message = $"{created.Count} bus reservation(s) created successfully."
        };
    }

    public async Task<ApiResponse<CreateBusReservationResponse>> UpdateAsync(UpdateBusReservationCommand command, bool isAdmin, CancellationToken cancellationToken = default)
    {
        BusReservation? existing = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException($"Bus reservation with id '{command.ReservationId}' was not found.");
        }

        if (!isAdmin && existing.RegistrationId != command.RequestingRegistrationId)
        {
            throw new BusinessRuleException("Only the person who made this reservation can modify it.");
        }

        BusReservation newReservation = await CreateInternalAsync(command.NewBusId, existing.RegistrationId, existing.Id, sendEmail: true, cancellationToken);

        _reservationRepository.Delete(existing);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        return new ApiResponse<CreateBusReservationResponse>
        {
            Success = true,
            Data = new CreateBusReservationResponse { Reservation = ToDto(newReservation) },
            Errors = [],
            Message = "Bus reservation updated successfully."
        };
    }

    public async Task<ApiResponse<DeleteBusReservationResponse>> DeleteAsync(Guid id, Guid requestingRegistrationId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        BusReservation? reservation = await _reservationRepository.GetByIdAsync(id, cancellationToken);

        if (reservation is null)
        {
            return new ApiResponse<DeleteBusReservationResponse> { Success = true, Data = new DeleteBusReservationResponse { Id = id, Deleted = false }, Errors = [], Message = "Reservation not found." };
        }

        if (!isAdmin && reservation.RegistrationId != requestingRegistrationId)
        {
            throw new BusinessRuleException("Only the person who made this reservation can cancel it.");
        }

        Guid cancelledRegistrationId = reservation.RegistrationId;

        _reservationRepository.Delete(reservation);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.BusCancelled, cancelledRegistrationId, cancellationToken);
        }
        catch
        {
            // Non-critical: the cancellation is already saved even if the email fails to send.
        }

        return new ApiResponse<DeleteBusReservationResponse> { Success = true, Data = new DeleteBusReservationResponse { Id = id, Deleted = true }, Errors = [], Message = "Reservation cancelled successfully." };
    }

    private async Task<BusReservation> CreateInternalAsync(Guid busId, Guid registrationId, Guid? excludeReservationId, bool sendEmail, CancellationToken cancellationToken)
    {
        Bus? bus = await _busRepository.GetByIdAsync(busId, cancellationToken);

        if (bus is null)
        {
            throw new NotFoundException($"Bus with id '{busId}' was not found.");
        }

        if (!bus.IsActive)
        {
            throw new BusinessRuleException("This bus is not currently open for reservations.");
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            throw new NotFoundException($"Registration with id '{registrationId}' was not found.");
        }

        HashSet<Guid> allowedPassTypeIds = bus.AllowedPassTypes.Select(p => p.PassTypeId).ToHashSet();

        if (allowedPassTypeIds.Count > 0 && !allowedPassTypeIds.Contains(registration.PassTypeId))
        {
            throw new BusinessRuleException("Your pass type is not allowed for this bus.");
        }

        if (registration.PaymentStatus is PaymentStatus.Unpaid or PaymentStatus.Failed)
        {
            throw new BusinessRuleException("You have not paid (fully or partially) for your registration yet.");
        }

        bool alreadyHasDirection = await _reservationRepository.HasReservationForDirectionAsync(registrationId, bus.Direction, excludeReservationId, cancellationToken);

        if (alreadyHasDirection)
        {
            throw new BusinessRuleException($"You already have a {bus.Direction.ToString().ToLowerInvariant()} bus reservation.");
        }

        int occupied = await _reservationRepository.GetOccupiedCountAsync(busId, cancellationToken);

        if (occupied >= bus.Capacity)
        {
            throw new BusinessRuleException("This bus is already full.");
        }

        BusReservation reservation = new()
        {
            BusId = busId,
            RegistrationId = registrationId
        };

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _reservationRepository.SaveChangesAsync(cancellationToken);

        if (sendEmail)
        {
            try
            {
                await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.BusConfirmed, registrationId, cancellationToken);
            }
            catch
            {
                // Non-critical: the reservation is already saved even if the email fails to send.
            }
        }

        BusReservation? saved = await _reservationRepository.GetByIdAsync(reservation.Id, cancellationToken);

        return saved ?? reservation;
    }

    private static BusReservationDto ToDto(BusReservation reservation)
    {
        return new BusReservationDto
        {
            Id = reservation.Id,
            BusId = reservation.BusId,
            Direction = reservation.Bus?.Direction ?? default,
            DepartureTime = reservation.Bus?.DepartureTime ?? default,
            PickupLocation = reservation.Bus?.PickupLocation ?? string.Empty,
            DestinationLocation = reservation.Bus?.DestinationLocation ?? string.Empty,
            RegistrationId = reservation.RegistrationId,
            RegistrationName = reservation.Registration is not null ? $"{reservation.Registration.FirstName} {reservation.Registration.LastName}" : null,
            RegistrationEmail = reservation.Registration?.Email
        };
    }
}