namespace Alakai.FestivalManager.Application.Features.ProductionBuildings.Services;

public class ProductionAccommodationBuildingService : IProductionAccommodationBuildingService
{
    private readonly CreateBuildingHandler _createHandler;
    private readonly GetBuildingByIdHandler _getByIdHandler;
    private readonly GetBuildingsHandler _getAllHandler;
    private readonly GetBuildingsByEditionIdHandler _getByEditionIdHandler;
    private readonly UpdateBuildingHandler _updateHandler;
    private readonly DeleteBuildingHandler _deleteHandler;
    private readonly IValidator<CreateBuildingCommand> _createValidator;
    private readonly IValidator<UpdateBuildingCommand> _updateValidator;

    public ProductionAccommodationBuildingService(CreateBuildingHandler createHandler, GetBuildingByIdHandler getByIdHandler, GetBuildingsHandler getAllHandler, GetBuildingsByEditionIdHandler getByEditionIdHandler, UpdateBuildingHandler updateHandler, DeleteBuildingHandler deleteHandler, IValidator<CreateBuildingCommand> createValidator, IValidator<UpdateBuildingCommand> updateValidator)
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

    public async Task<ApiResponse<CreateProductionAccommodationBuildingResponse>> CreateAsync(CreateBuildingCommand command, CancellationToken cancellationToken = default)
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
        GetBuildingByIdQuery query = new(id);
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
        GetBuildingsQuery query = new();
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
        GetBuildingsByEditionIdQuery query = new(editionId);
        IReadOnlyList<ProductionAccommodationBuildingDto> dtos = await _getByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionAccommodationBuildingsResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} production accommodation buildings registered at this edition.",
            Data = new GetProductionAccommodationBuildingsResponse { ProductionAccommodationBuildings = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionAccommodationBuildingResponse>> UpdateAsync(UpdateBuildingCommand command, CancellationToken cancellationToken = default)
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
        DeleteBuildingCommand command = new(id);
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