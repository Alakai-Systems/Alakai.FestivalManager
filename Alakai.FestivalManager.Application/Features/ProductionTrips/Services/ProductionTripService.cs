namespace Alakai.FestivalManager.Application.Features.ProductionTrips.Services;

public class ProductionTripService : IProductionTripService
{
    private readonly CreateTripHandler _createHandler;
    private readonly GetTripByIdHandler _getByIdHandler;
    private readonly GetTripsByEditionIdHandler _getByEditionIdHandler;
    private readonly UpdateTripHandler _updateHandler;
    private readonly DeleteTripHandler _deleteHandler;
    private readonly IValidator<CreateTripCommand> _createValidator;
    private readonly IValidator<UpdateTripCommand> _updateValidator;

    public ProductionTripService(CreateTripHandler createHandler, GetTripByIdHandler getByIdHandler, GetTripsByEditionIdHandler getByEditionIdHandler, UpdateTripHandler updateHandler, DeleteTripHandler deleteHandler, IValidator<CreateTripCommand> createValidator, IValidator<UpdateTripCommand> updateValidator)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _getByEditionIdHandler = getByEditionIdHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<CreateTripResponse>> CreateAsync(CreateTripCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionTripDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateTripResponse>
        {
            Success = true,
            Message = "Trip created successfully.",
            Data = new CreateTripResponse { Trip = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetTripResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetTripByIdQuery query = new(id);
        ProductionTripDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Trip with id '{id}' was not found."); }

        return new ApiResponse<GetTripResponse>
        {
            Success = true,
            Message = $"Trip with id '{id}' was found.",
            Data = new GetTripResponse { Trip = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetTripsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetTripsByEditionIdQuery query = new(editionId);
        IReadOnlyList<ProductionTripDto> dtos = await _getByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetTripsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} trips registered for this edition.",
            Data = new GetTripsResponse { Trips = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateTripResponse>> UpdateAsync(UpdateTripCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionTripDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateTripResponse>
        {
            Success = true,
            Message = "Trip updated successfully.",
            Data = new UpdateTripResponse { Trip = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteTripResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteTripCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteTripResponse>
        {
            Success = true,
            Message = "Trip deleted successfully.",
            Data = new DeleteTripResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}