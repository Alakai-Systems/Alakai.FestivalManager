namespace Alakai.FestivalManager.Application.Features.DiscountCodes.Services;

public class DiscountCodeService : IDiscountCodeService
{
    private readonly CreateDiscountCodeHandler _createDiscountCodeHandler;
    private readonly GetDiscountCodeByIdHandler _getDiscountCodeByIdHandler;
    private readonly GetDiscountCodesHandler _getDiscountCodesHandler;
    private readonly GetDiscountCodesByEditionIdHandler _getDiscountCodesByEditionIdHandler;
    private readonly UpdateDiscountCodeHandler _updateDiscountCodeHandler;
    private readonly DeleteDiscountCodeHandler _deleteDiscountCodeHandler;
    private readonly IValidator<CreateDiscountCodeCommand> _createDiscountCodeValidator;
    private readonly IValidator<UpdateDiscountCodeCommand> _updateDiscountCodeValidator;

    public DiscountCodeService(CreateDiscountCodeHandler createDiscountCodeHandler, GetDiscountCodeByIdHandler getDiscountCodeByIdHandler, GetDiscountCodesHandler getDiscountCodesHandler, GetDiscountCodesByEditionIdHandler getDiscountCodesByEditionIdHandler, UpdateDiscountCodeHandler updateDiscountCodeHandler, DeleteDiscountCodeHandler deleteDiscountCodeHandler, IValidator<CreateDiscountCodeCommand> createDiscountCodeValidator, IValidator<UpdateDiscountCodeCommand> updateDiscountCodeValidator)
    {
        _createDiscountCodeHandler = createDiscountCodeHandler;
        _getDiscountCodeByIdHandler = getDiscountCodeByIdHandler;
        _getDiscountCodesHandler = getDiscountCodesHandler;
        _getDiscountCodesByEditionIdHandler = getDiscountCodesByEditionIdHandler;
        _updateDiscountCodeHandler = updateDiscountCodeHandler;
        _deleteDiscountCodeHandler = deleteDiscountCodeHandler;
        _createDiscountCodeValidator = createDiscountCodeValidator;
        _updateDiscountCodeValidator = updateDiscountCodeValidator;
    }

    public async Task<ApiResponse<CreateDiscountCodeResponse>> CreateAsync(CreateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createDiscountCodeValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        DiscountCodeDto discountCodeDto = await _createDiscountCodeHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new CreateDiscountCodeResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} is correctly registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodeByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DiscountCodeDto discountCodeDto = await _getDiscountCodeByIdHandler.HandleAsync(new GetDiscountCodeByIdQuery { Id = id }, cancellationToken);

        ApiResponse<GetDiscountCodeByIdResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodeByIdResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} retrieved successfully"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCodeDto> discountCodes = await _getDiscountCodesHandler.HandleAsync(cancellationToken);

        ApiResponse<GetDiscountCodesResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodesResponse { DiscountCodes = discountCodes },
            Errors = [],
            Message = $"There are {discountCodes.Count} discount codes registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetDiscountCodesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DiscountCodeDto> discountCodes = await _getDiscountCodesByEditionIdHandler.HandleAsync(new GetDiscountCodesByEditionIdQuery { EditionId = editionId }, cancellationToken);

        ApiResponse<GetDiscountCodesByEditionIdResponse> response = new()
        {
            Success = true,
            Data = new GetDiscountCodesByEditionIdResponse { DiscountCodes = discountCodes },
            Errors = [],
            Message = $"There are {discountCodes.Count} discount codes for this edition"
        };

        return response;
    }

    public async Task<ApiResponse<UpdateDiscountCodeResponse>> UpdateAsync(Guid id, UpdateDiscountCodeCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        if (command.ActivationType == DiscountActivationType.Immediate)
        {
            command.ActivationThreshold = null;
        }

        ValidationResult validationResult = await _updateDiscountCodeValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        DiscountCodeDto discountCodeDto = await _updateDiscountCodeHandler.HandleAsync(command, cancellationToken);

        ApiResponse<UpdateDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new UpdateDiscountCodeResponse { DiscountCode = discountCodeDto },
            Errors = [],
            Message = $"{discountCodeDto.Code} updated successfully"
        };

        return response;
    }

    public async Task<ApiResponse<DeleteDiscountCodeResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteDiscountCodeHandler.HandleAsync(new DeleteDiscountCodeCommand { Id = id }, cancellationToken);

        ApiResponse<DeleteDiscountCodeResponse> response = new()
        {
            Success = true,
            Data = new DeleteDiscountCodeResponse { Id = id, Deleted = true },
            Errors = [],
            Message = "Discount code deleted successfully"
        };

        return response;
    }
}
