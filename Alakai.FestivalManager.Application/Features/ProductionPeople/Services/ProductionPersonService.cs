namespace Alakai.FestivalManager.Application.Features.ProductionPeople.Services;

public class ProductionPersonService : IProductionPersonService
{
    private readonly CreateProductionPersonHandler _createProductionPersonHandler;
    private readonly GetProductionPersonByIdHandler _getProductionPersonByIdHandler;
    private readonly GetProductionPeopleHandler _getProductionPeopleHandler;
    private readonly GetProductionPeopleByEditionIdHandler _getProductionPeopleByEditionIdHandler;
    private readonly UpdateProductionPersonHandler _updateProductionPersonHandler;
    private readonly DeleteProductionPersonHandler _deleteProductionPersonHandler;
    private readonly IValidator<CreateProductionPersonCommand> _createProductionPersonValidator;
    private readonly IValidator<UpdateProductionPersonCommand> _updateProductionPersonValidator;

    public ProductionPersonService(CreateProductionPersonHandler createProductionPersonHandler, GetProductionPersonByIdHandler getProductionPersonByIdHandler, GetProductionPeopleHandler getProductionPeopleHandler, GetProductionPeopleByEditionIdHandler getProductionPeopleByEditionIdHandler, UpdateProductionPersonHandler updateProductionPersonHandler, DeleteProductionPersonHandler deleteProductionPersonHandler, IValidator<CreateProductionPersonCommand> createProductionPersonValidator, IValidator<UpdateProductionPersonCommand> updateProductionPersonValidator)
    {
        _createProductionPersonHandler = createProductionPersonHandler;
        _getProductionPersonByIdHandler = getProductionPersonByIdHandler;
        _getProductionPeopleHandler = getProductionPeopleHandler;
        _getProductionPeopleByEditionIdHandler = getProductionPeopleByEditionIdHandler;
        _updateProductionPersonHandler = updateProductionPersonHandler;
        _deleteProductionPersonHandler = deleteProductionPersonHandler;
        _createProductionPersonValidator = createProductionPersonValidator;
        _updateProductionPersonValidator = updateProductionPersonValidator;
    }

    public async Task<ApiResponse<CreateProductionPersonResponse>> CreateAsync(CreateProductionPersonCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createProductionPersonValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        ProductionPersonDto productionPersonDto = await _createProductionPersonHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<CreateProductionPersonResponse>
        {
            Success = true,
            Message = "Production person created successfully.",
            Data = new CreateProductionPersonResponse { ProductionPerson = productionPersonDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionPersonByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetProductionPersonByIdQuery query = new(id);

        ProductionPersonDto? productionPersonDto = await _getProductionPersonByIdHandler.HandleAsync(query, cancellationToken);

        if (productionPersonDto is null)
        {
            throw new NotFoundException($"Production person with id '{id}' was not found.");
        }

        return new ApiResponse<GetProductionPersonByIdResponse>
        {
            Success = true,
            Message = $"Production person with id '{id}' was found.",
            Data = new GetProductionPersonByIdResponse { ProductionPerson = productionPersonDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionPeopleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetProductionPeopleQuery query = new();

        IReadOnlyList<ProductionPersonDto> productionPeopleDtos = await _getProductionPeopleHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionPeopleResponse>
        {
            Success = true,
            Message = $"There are {productionPeopleDtos.Count} production people registered.",
            Data = new GetProductionPeopleResponse { ProductionPeople = productionPeopleDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<GetProductionPeopleResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetProductionPeopleByEditionIdQuery query = new(editionId);

        IReadOnlyList<ProductionPersonDto> productionPeopleDtos = await _getProductionPeopleByEditionIdHandler.HandleAsync(query, cancellationToken);

        return new ApiResponse<GetProductionPeopleResponse>
        {
            Success = true,
            Message = $"There are {productionPeopleDtos.Count} production people registered at this edition.",
            Data = new GetProductionPeopleResponse { ProductionPeople = productionPeopleDtos },
            Errors = []
        };
    }

    public async Task<ApiResponse<UpdateProductionPersonResponse>> UpdateAsync(UpdateProductionPersonCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateProductionPersonValidator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        ProductionPersonDto productionPersonDto = await _updateProductionPersonHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateProductionPersonResponse>
        {
            Success = true,
            Message = "Production person updated successfully.",
            Data = new UpdateProductionPersonResponse { ProductionPerson = productionPersonDto },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteProductionPersonResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteProductionPersonCommand command = new(id);

        Guid deletedId = await _deleteProductionPersonHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteProductionPersonResponse>
        {
            Success = true,
            Message = "Production person deleted successfully.",
            Data = new DeleteProductionPersonResponse { Id = deletedId, Deleted = true },
            Errors = []
        };
    }
}