namespace Alakai.FestivalManager.Application.Features.ProductionAccommodations.Services;

public class ProductionAccommodationService : IProductionAccommodationService
{
    private readonly GetProductionAccommodationsHandler _getAllHandler;
    private readonly CreateProductionAccommodationHandler _createHandler;
    private readonly GetProductionAccommodationByIdHandler _getByIdHandler;
    private readonly GetProductionAccommodationsByZoneIdHandler _getByZoneIdHandler;
    private readonly UpdateProductionAccommodationHandler _updateHandler;
    private readonly DeleteProductionAccommodationHandler _deleteHandler;
    private readonly IValidator<CreateProductionAccommodationCommand> _createValidator;
    private readonly IValidator<UpdateProductionAccommodationCommand> _updateValidator;

    public ProductionAccommodationService(GetProductionAccommodationsHandler getAllHandler, CreateProductionAccommodationHandler createHandler, GetProductionAccommodationByIdHandler getByIdHandler, GetProductionAccommodationsByZoneIdHandler getByZoneIdHandler, UpdateProductionAccommodationHandler updateHandler, DeleteProductionAccommodationHandler deleteHandler, IValidator<CreateProductionAccommodationCommand> createValidator, IValidator<UpdateProductionAccommodationCommand> updateValidator)
    {
        _getAllHandler = getAllHandler;
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _getByZoneIdHandler = getByZoneIdHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<GetProductionAccommodationsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationsQuery query = new();
        IReadOnlyList<ProductionAccommodationDto> dtos = await _getAllHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} accommodations registered.",
            Data = new GetProductionAccommodationsResponse { ProductionAccommodations = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<CreateProductionAccommodationResponse>> CreateAsync(CreateProductionAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateProductionAccommodationResponse>
        {
            Success = true,
            Message = "Production accommodation created successfully.",
            Data = new CreateProductionAccommodationResponse { ProductionAccommodation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationByIdQuery query = new(id);
        ProductionAccommodationDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Production accommodation with id '{id}' was not found."); }

        return new ApiResponse<GetProductionAccommodationByIdResponse>
        {
            Success = true,
            Message = $"Production accommodation with id '{id}' was found.",
            Data = new GetProductionAccommodationByIdResponse { ProductionAccommodation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationsResponse>> GetByZoneIdAsync(Guid zoneId, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationsByZoneIdQuery query = new(zoneId);
        IReadOnlyList<ProductionAccommodationDto> dtos = await _getByZoneIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} accommodations registered for this zone.",
            Data = new GetProductionAccommodationsResponse { ProductionAccommodations = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionAccommodationResponse>> UpdateAsync(UpdateProductionAccommodationCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateProductionAccommodationResponse>
        {
            Success = true,
            Message = "Production accommodation updated successfully.",
            Data = new UpdateProductionAccommodationResponse { ProductionAccommodation = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteProductionAccommodationResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteProductionAccommodationCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteProductionAccommodationResponse>
        {
            Success = true,
            Message = "Production accommodation deleted successfully.",
            Data = new DeleteProductionAccommodationResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}