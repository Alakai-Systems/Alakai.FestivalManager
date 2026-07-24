namespace Alakai.FestivalManager.Application.Features.ProductionReservations.Services;

public class ProductionReservationService : IProductionReservationService
{
    private readonly GetReservationsHandler _getAllHandler;
    private readonly CreateReservationHandler _createHandler;
    private readonly GetReservationByIdHandler _getByIdHandler;
    private readonly GetReservationsByBuildingIdHandler _getByBuildingIdHandler;
    private readonly UpdateReservationHandler _updateHandler;
    private readonly DeleteReservationHandler _deleteHandler;
    private readonly IValidator<CreateReservationCommand> _createValidator;
    private readonly IValidator<UpdateReservationCommand> _updateValidator;

    public ProductionReservationService(GetReservationsHandler getAllHandler, CreateReservationHandler createHandler, GetReservationByIdHandler getByIdHandler, GetReservationsByBuildingIdHandler getByBuildingIdHandler, UpdateReservationHandler updateHandler, DeleteReservationHandler deleteHandler, IValidator<CreateReservationCommand> createValidator, IValidator<UpdateReservationCommand> updateValidator)
    {
        _getAllHandler = getAllHandler;
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _getByBuildingIdHandler = getByBuildingIdHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<GetReservationsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetReservationsQuery query = new();
        IReadOnlyList<ReservationDto> dtos = await _getAllHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetReservationsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} reservations registered.",
            Data = new GetReservationsResponse { Reservations = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<CreateReservationResponse>> CreateAsync(CreateReservationCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ReservationDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateReservationResponse>
        {
            Success = true,
            Message = "Reservation created successfully.",
            Data = new CreateReservationResponse { Reservation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetReservationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetReservationByIdQuery query = new(id);
        ReservationDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Reservation with id '{id}' was not found."); }

        return new ApiResponse<GetReservationResponse>
        {
            Success = true,
            Message = $"Reservation with id '{id}' was found.",
            Data = new GetReservationResponse { Reservation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetReservationsResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        GetReservationsByBuildingIdQuery query = new(buildingId);
        IReadOnlyList<ReservationDto> dtos = await _getByBuildingIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetReservationsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} reservations registered for this building.",
            Data = new GetReservationsResponse { Reservations = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateReservationResponse>> UpdateAsync(UpdateReservationCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ReservationDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateReservationResponse>
        {
            Success = true,
            Message = "Reservation updated successfully.",
            Data = new UpdateReservationResponse { Reservation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteReservationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteReservationCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteReservationResponse>
        {
            Success = true,
            Message = "Reservation deleted successfully.",
            Data = new DeleteReservationResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}