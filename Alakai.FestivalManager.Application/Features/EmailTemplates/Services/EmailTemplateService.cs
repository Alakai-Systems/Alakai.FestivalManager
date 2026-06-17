namespace Alakai.FestivalManager.Application.Features.EmailTemplates.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private readonly CreateEmailTemplateHandler _createEmailTemplateHandler;
    private readonly GetEmailTemplateByIdHandler _getEmailTemplateByIdHandler;
    private readonly GetEmailTemplatesHandler _getEmailTemplatesHandler;
    private readonly GetEmailTemplatesByEditionIdHandler _getEmailTemplatesByEditionIdHandler;
    private readonly UpdateEmailTemplateHandler _updateEmailTemplateHandler;
    private readonly DeleteEmailTemplateHandler _deleteEmailTemplateHandler;
    private readonly IValidator<CreateEmailTemplateCommand> _createEmailTemplateValidator;
    private readonly IValidator<UpdateEmailTemplateCommand> _updateEmailTemplateValidator;

    public EmailTemplateService(
        CreateEmailTemplateHandler createEmailTemplateHandler,
        GetEmailTemplateByIdHandler getEmailTemplateByIdHandler,
        GetEmailTemplatesHandler getEmailTemplatesHandler,
        GetEmailTemplatesByEditionIdHandler getEmailTemplatesByEditionIdHandler,
        UpdateEmailTemplateHandler updateEmailTemplateHandler,
        DeleteEmailTemplateHandler deleteEmailTemplateHandler,
        IValidator<CreateEmailTemplateCommand> createEmailTemplateValidator,
        IValidator<UpdateEmailTemplateCommand> updateEmailTemplateValidator)
    {
        _createEmailTemplateHandler = createEmailTemplateHandler;
        _getEmailTemplateByIdHandler = getEmailTemplateByIdHandler;
        _getEmailTemplatesHandler = getEmailTemplatesHandler;
        _getEmailTemplatesByEditionIdHandler = getEmailTemplatesByEditionIdHandler;
        _updateEmailTemplateHandler = updateEmailTemplateHandler;
        _deleteEmailTemplateHandler = deleteEmailTemplateHandler;
        _createEmailTemplateValidator = createEmailTemplateValidator;
        _updateEmailTemplateValidator = updateEmailTemplateValidator;
    }

    public async Task<ApiResponse<CreateEmailTemplateResponse>> CreateAsync(CreateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createEmailTemplateValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EmailTemplateDto emailTemplateDto = await _createEmailTemplateHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateEmailTemplateResponse> response = new()
        {
            Success = true,
            Data = new CreateEmailTemplateResponse()
            {
                EmailTemplate = emailTemplateDto,
            },
            Errors = new List<string>(),
            Message = $"{emailTemplateDto.Name} is correctly registered",
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailTemplateByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        GetEmailTemplateByIdQuery query = new(id);

        EmailTemplateDto? emailTemplateDto = await _getEmailTemplateByIdHandler.HandleAsync(query, cancellationToken);

        if (emailTemplateDto is null)
        {
            throw new NotFoundException($"Email template with id '{id}' was not found.");
        }

        ApiResponse<GetEmailTemplateByIdResponse> response = new()
        {
            Success = true,
            Data = new GetEmailTemplateByIdResponse()
            {
                EmailTemplate = emailTemplateDto,
            },
            Errors = new List<string>(),
            Message = $"{emailTemplateDto.Name} it's in our system",
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailTemplatesResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        GetEmailTemplatesQuery query = new();

        IReadOnlyList<EmailTemplateDto> emailTemplateDtos = await _getEmailTemplatesHandler.HandleAsync(query, cancellationToken);

        if (emailTemplateDtos is null)
        {
            throw new NotFoundException($"There are no email templates created yet.");
        }

        ApiResponse<GetEmailTemplatesResponse> response = new()
        {
            Success = true,
            Data = new GetEmailTemplatesResponse()
            {
                EmailTemplates = emailTemplateDtos,
            },
            Errors = new List<string>(),
            Message = $"There are {emailTemplateDtos.Count} email templates registered",
        };

        return response;
    }

    public async Task<ApiResponse<GetEmailTemplatesByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        GetEmailTemplatesByEditionIdQuery query = new(editionId);

        IReadOnlyList<EmailTemplateDto> emailTemplateDtos = await _getEmailTemplatesByEditionIdHandler.HandleAsync(query, cancellationToken);

        ApiResponse<GetEmailTemplatesByEditionIdResponse> response = new()
        {
            Success = true,
            Data = new GetEmailTemplatesByEditionIdResponse()
            {
                EmailTemplates = emailTemplateDtos,
            },
            Errors = new List<string>(),
            Message = $"There are {emailTemplateDtos.Count} email templates for this edition",
        };

        return response;
    }

    public async Task<ApiResponse<UpdateEmailTemplateResponse>> UpdateAsync(UpdateEmailTemplateCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateEmailTemplateValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        EmailTemplateDto emailTemplateDto = await _updateEmailTemplateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<UpdateEmailTemplateResponse>
        {
            Success = true,
            Message = "Email template updated successfully.",
            Data = new UpdateEmailTemplateResponse
            {
                EmailTemplate = emailTemplateDto
            },
            Errors = []
        };
    }

    public async Task<ApiResponse<DeleteEmailTemplateResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        DeleteEmailTemplateCommand command = new(id);

        Guid deletedId = await _deleteEmailTemplateHandler.HandleAsync(command, cancellationToken);

        return new ApiResponse<DeleteEmailTemplateResponse>
        {
            Success = true,
            Message = "Email template deleted successfully.",
            Data = new DeleteEmailTemplateResponse
            {
                Id = deletedId,
                Deleted = true
            },
            Errors = []
        };
    }
}
