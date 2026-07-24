namespace Alakai.FestivalManager.Application.Features.ProductionSuppliers.Services;

public class ProductionSupplierService : IProductionSupplierService
{
    private readonly CreateProductionSupplierHandler _createHandler;
    private readonly GetProductionSupplierByIdHandler _getByIdHandler;
    private readonly GetProductionSuppliersHandler _getAllHandler;
    private readonly GetProductionSuppliersByEditionIdHandler _getByEditionIdHandler;
    private readonly UpdateProductionSupplierHandler _updateHandler;
    private readonly DeleteProductionSupplierHandler _deleteHandler;
    private readonly IValidator<CreateProductionSupplierCommand> _createValidator;
    private readonly IValidator<UpdateProductionSupplierCommand> _updateValidator;

    public ProductionSupplierService(CreateProductionSupplierHandler createHandler, GetProductionSupplierByIdHandler getByIdHandler, GetProductionSuppliersHandler getAllHandler, GetProductionSuppliersByEditionIdHandler getByEditionIdHandler, UpdateProductionSupplierHandler updateHandler, DeleteProductionSupplierHandler deleteHandler, IValidator<CreateProductionSupplierCommand> createValidator, IValidator<UpdateProductionSupplierCommand> updateValidator)
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

    public async Task<ApiResponse<CreateProductionSupplierResponse>> CreateAsync(CreateProductionSupplierCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        ProductionSupplierDto dto = await _createHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateProductionSupplierResponse>
        {
            Success = true,
            Message = "Production supplier created successfully.",
            Data = new CreateProductionSupplierResponse { ProductionSupplier = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionSupplierByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetProductionSupplierByIdQuery query = new(id);
        ProductionSupplierDto? dto = await _getByIdHandler.HandleAsync(query, cancellationToken);

        if (dto is null)
        {
            throw new NotFoundException($"Production supplier with id '{id}' was not found.");
        }

        return new ApiResponse<GetProductionSupplierByIdResponse>
        {
            Success = true,
            Message = $"Production supplier with id '{id}' was found.",
            Data = new GetProductionSupplierByIdResponse { ProductionSupplier = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionSuppliersResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetProductionSuppliersQuery query = new();
        IReadOnlyList<ProductionSupplierDto> dtos = await _getAllHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionSuppliersResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} production suppliers registered.",
            Data = new GetProductionSuppliersResponse { ProductionSuppliers = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionSuppliersResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetProductionSuppliersByEditionIdQuery query = new(editionId);
        IReadOnlyList<ProductionSupplierDto> dtos = await _getByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionSuppliersResponse>
        {
            Success = true,
            Message = $"There are {dtos.Count} production suppliers registered at this edition.",
            Data = new GetProductionSuppliersResponse { ProductionSuppliers = dtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionSupplierResponse>> UpdateAsync(UpdateProductionSupplierCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        ProductionSupplierDto dto = await _updateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateProductionSupplierResponse>
        {
            Success = true,
            Message = "Production supplier updated successfully.",
            Data = new UpdateProductionSupplierResponse { ProductionSupplier = dto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteProductionSupplierResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteProductionSupplierCommand command = new(id);
        Guid deletedId = await _deleteHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteProductionSupplierResponse>
        {
            Success = true,
            Message = "Production supplier deleted successfully.",
            Data = new DeleteProductionSupplierResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}