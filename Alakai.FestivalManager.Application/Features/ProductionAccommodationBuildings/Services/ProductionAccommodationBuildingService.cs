namespace Alakai.FestivalManager.Application.Features.ProductionAccommodationBuildings.Services;

public class ProductionAccommodationBuildingService : IProductionAccommodationBuildingService
{
    private readonly CreateProductionAccommodationBuildingHandler _createHandler;
    private readonly GetProductionAccommodationBuildingByIdHandler _getByIdHandler;
    private readonly GetProductionAccommodationBuildingsHandler _getAllHandler;
    private readonly GetProductionAccommodationBuildingsByEditionIdHandler _getByEditionIdHandler;
    private readonly UpdateProductionAccommodationBuildingHandler _updateHandler;
    private readonly DeleteProductionAccommodationBuildingHandler _deleteHandler;
    private readonly IValidator<CreateProductionAccommodationBuildingCommand> _createValidator;
    private readonly IValidator<UpdateProductionAccommodationBuildingCommand> _updateValidator;

    public ProductionAccommodationBuildingService(CreateProductionAccommodationBuildingHandler createHandler, GetProductionAccommodationBuildingByIdHandler getByIdHandler, GetProductionAccommodationBuildingsHandler getAllHandler, GetProductionAccommodationBuildingsByEditionIdHandler getByEditionIdHandler, UpdateProductionAccommodationBuildingHandler updateHandler, DeleteProductionAccommodationBuildingHandler deleteHandler, IValidator<CreateProductionAccommodationBuildingCommand> createValidator, IValidator<UpdateProductionAccommodationBuildingCommand> updateValidator)
    {
        _createHandler = createHandler;
        _getByIdHandler = getByIdHandler;
        _getAllHandler = getAllHandler;
        _getByEditionIdHandler = getByEditionIdHandler;
        _updateHandler = updateHandler;
        _deleteHandler = deleteHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<ApiResponse<CreateProductionAccommodationBuildingResponse>> CreateAsync(CreateProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationBuildingDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateProductionAccommodationBuildingResponse>
        {
            Success = true,
            Message = "Production accommodation building created successfully.",
            Data = new CreateProductionAccommodationBuildingResponse { ProductionAccommodationBuilding = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationBuildingByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationBuildingByIdQuery query = new(id);
        ProductionAccommodationBuildingDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null) { throw new NotFoundException($"Production accommodation building with id '{id}' was not found."); }

        return new ApiResponse<GetProductionAccommodationBuildingByIdResponse>
        {
            Success = true,
            Message = $"Production accommodation building with id '{id}' was found.",
            Data = new GetProductionAccommodationBuildingByIdResponse { ProductionAccommodationBuilding = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationBuildingsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationBuildingsQuery query = new();
        IReadOnlyList<ProductionAccommodationBuildingDto> dtos = await _getAllHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationBuildingsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} production accommodation buildings registered.",
            Data = new GetProductionAccommodationBuildingsResponse { ProductionAccommodationBuildings = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionAccommodationBuildingsResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetProductionAccommodationBuildingsByEditionIdQuery query = new(editionId);
        IReadOnlyList<ProductionAccommodationBuildingDto> dtos = await _getByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationBuildingsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} production accommodation buildings registered at this edition.",
            Data = new GetProductionAccommodationBuildingsResponse { ProductionAccommodationBuildings = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionAccommodationBuildingResponse>> UpdateAsync(UpdateProductionAccommodationBuildingCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid) { throw new ValidationException(validationResult.Errors); }

        ProductionAccommodationBuildingDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateProductionAccommodationBuildingResponse>
        {
            Success = true,
            Message = "Production accommodation building updated successfully.",
            Data = new UpdateProductionAccommodationBuildingResponse { ProductionAccommodationBuilding = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteProductionAccommodationBuildingResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteProductionAccommodationBuildingCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteProductionAccommodationBuildingResponse>
        {
            Success = true,
            Message = "Production accommodation building deleted successfully.",
            Data = new DeleteProductionAccommodationBuildingResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}