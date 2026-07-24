namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationZones.Services;

public class ProductionAccommodationZoneService : IProductionAccommodationZoneService
{
    private readonly GetProductionAccommodationZonesHandler _getAllHandler;
    private readonly CreateProductionAccommodationZoneHandler _createHandler;
    private readonly GetProductionAccommodationZoneByIdHandler _getByIdHandler;
    private readonly GetProductionAccommodationZonesByBuildingIdHandler _getByBuildingIdHandler;
    private readonly UpdateProductionAccommodationZoneHandler _updateHandler;
    private readonly DeleteProductionAccommodationZoneHandler _deleteHandler;
    private readonly IValidator<CreateProductionAccommodationZoneCommand> _createValidator;
    private readonly IValidator<UpdateProductionAccommodationZoneCommand> _updateValidator;

    public ProductionAccommodationZoneService(GetProductionAccommodationZonesHandler getAllHandler, CreateProductionAccommodationZoneHandler createHandler, GetProductionAccommodationZoneByIdHandler getByIdHandler, GetProductionAccommodationZonesByBuildingIdHandler getByBuildingIdHandler, UpdateProductionAccommodationZoneHandler updateHandler, DeleteProductionAccommodationZoneHandler deleteHandler, IValidator<CreateProductionAccommodationZoneCommand> createValidator, IValidator<UpdateProductionAccommodationZoneCommand> updateValidator)
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

    public async Task<ApiResponse<GetProductionAccommodationZonesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationZonesQuery query = new();
        IReadOnlyList<ProductionAccommodationZoneDto> dtos = await _getAllHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationZonesResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} zones registered.",
            Data = new GetProductionAccommodationZonesResponse { ProductionAccommodationZones = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<CreateProductionAccommodationZoneResponse>> CreateAsync(CreateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationZoneDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateProductionAccommodationZoneResponse>
        {
            Success = true,
            Message = "Production accommodation zone created successfully.",
            Data = new CreateProductionAccommodationZoneResponse { ProductionAccommodationZone = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationZoneByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationZoneByIdQuery query = new(id);
        ProductionAccommodationZoneDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Production accommodation zone with id '{id}' was not found."); }

        return new ApiResponse<GetProductionAccommodationZoneByIdResponse>
        {
            Success = true,
            Message = $"Production accommodation zone with id '{id}' was found.",
            Data = new GetProductionAccommodationZoneByIdResponse { ProductionAccommodationZone = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationZonesResponse>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationZonesByBuildingIdQuery query = new(buildingId);
        IReadOnlyList<ProductionAccommodationZoneDto> dtos = await _getByBuildingIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationZonesResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} zones registered for this building.",
            Data = new GetProductionAccommodationZonesResponse { ProductionAccommodationZones = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionAccommodationZoneResponse>> UpdateAsync(UpdateProductionAccommodationZoneCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationZoneDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateProductionAccommodationZoneResponse>
        {
            Success = true,
            Message = "Production accommodation zone updated successfully.",
            Data = new UpdateProductionAccommodationZoneResponse { ProductionAccommodationZone = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteProductionAccommodationZoneResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteProductionAccommodationZoneCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteProductionAccommodationZoneResponse>
        {
            Success = true,
            Message = "Production accommodation zone deleted successfully.",
            Data = new DeleteProductionAccommodationZoneResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}