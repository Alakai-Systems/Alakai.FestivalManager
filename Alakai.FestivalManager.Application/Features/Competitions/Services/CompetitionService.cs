using Alakai.FestivalManager.Application.Features.Competitions.Contracts.DTOs;
using Alakai.FestivalManager.Application.Features.Competitions.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Competitions.Services;

public class CompetitionService : ICompetitionService
{
    private readonly CreateCompetitionHandler _createCompetitionHandler;
    private readonly GetCompetitionByIdHandler _getCompetitionByIdHandler;
    private readonly GetCompetitionsHandler _getCompetitionsHandler;
    private readonly GetCompetitionsByEditionIdHandler _getCompetitionsByEditionIdHandler;
    private readonly UpdateCompetitionHandler _updateCompetitionHandler;
    private readonly DeleteCompetitionHandler _deleteCompetitionHandler;
    private readonly IValidator<CreateCompetitionCommand> _createCompetitionValidator;
    private readonly IValidator<UpdateCompetitionCommand> _updateCompetitionValidator;

    public CompetitionService(CreateCompetitionHandler createCompetitionHandler, GetCompetitionByIdHandler getCompetitionByIdHandler, GetCompetitionsHandler getCompetitionsHandler, GetCompetitionsByEditionIdHandler getCompetitionsByEditionIdHandler, UpdateCompetitionHandler updateCompetitionHandler, DeleteCompetitionHandler deleteCompetitionHandler, IValidator<CreateCompetitionCommand> createCompetitionValidator, IValidator<UpdateCompetitionCommand> updateCompetitionValidator)
    {
        _createCompetitionHandler = createCompetitionHandler;
        _getCompetitionByIdHandler = getCompetitionByIdHandler;
        _getCompetitionsHandler = getCompetitionsHandler;
        _getCompetitionsByEditionIdHandler = getCompetitionsByEditionIdHandler;
        _updateCompetitionHandler = updateCompetitionHandler;
        _deleteCompetitionHandler = deleteCompetitionHandler;
        _createCompetitionValidator = createCompetitionValidator;
        _updateCompetitionValidator = updateCompetitionValidator;
    }

    public async Task<ApiResponse<CreateCompetitionResponse>> CreateAsync(CreateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createCompetitionValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        CompetitionDto competitionDto = await _createCompetitionHandler.HandleAsync(command, cancellationToken);

        ApiResponse<CreateCompetitionResponse> response = new()
        {
            Success = true,
            Data = new CreateCompetitionResponse
            {
                Competition = competitionDto
            },
            Errors = [],
            Message = $"{competitionDto.Name} is correctly registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        CompetitionDto competitionDto = await _getCompetitionByIdHandler.HandleAsync(new GetCompetitionByIdQuery { Id = id }, cancellationToken);

        ApiResponse<GetCompetitionByIdResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionByIdResponse
            {
                Competition = competitionDto
            },
            Errors = [],
            Message = $"{competitionDto.Name} retrieved successfully"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionsResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionDto> competitions = await _getCompetitionsHandler.HandleAsync(cancellationToken);

        ApiResponse<GetCompetitionsResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionsResponse
            {
                Competitions = competitions
            },
            Errors = [],
            Message = $"There are {competitions.Count} competitions registered"
        };

        return response;
    }

    public async Task<ApiResponse<GetCompetitionsByEditionIdResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CompetitionDto> competitions = await _getCompetitionsByEditionIdHandler.HandleAsync(new GetCompetitionsByEditionIdQuery { EditionId = editionId }, cancellationToken);

        ApiResponse<GetCompetitionsByEditionIdResponse> response = new()
        {
            Success = true,
            Data = new GetCompetitionsByEditionIdResponse
            {
                Competitions = competitions
            },
            Errors = [],
            Message = $"There are {competitions.Count} competitions for this edition"
        };

        return response;
    }

    public async Task<ApiResponse<UpdateCompetitionResponse>> UpdateAsync(Guid id, UpdateCompetitionCommand command, CancellationToken cancellationToken = default)
    {
        command.Id = id;

        ValidationResult validationResult = await _updateCompetitionValidator.ValidateAsync(command, cancellationToken);

        if (validationResult.IsValid is false)
        {
            throw new ValidationException(validationResult.Errors);
        }

        CompetitionDto competitionDto = await _updateCompetitionHandler.HandleAsync(command, cancellationToken);

        ApiResponse<UpdateCompetitionResponse> response = new()
        {
            Success = true,
            Data = new UpdateCompetitionResponse
            {
                Competition = competitionDto
            },
            Errors = [],
            Message = $"{competitionDto.Name} updated successfully"
        };

        return response;
    }

    public async Task<ApiResponse<DeleteCompetitionResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _deleteCompetitionHandler.HandleAsync(new DeleteCompetitionCommand { Id = id }, cancellationToken);

        ApiResponse<DeleteCompetitionResponse> response = new()
        {
            Success = true,
            Data = new DeleteCompetitionResponse
            {
                Id = id,
                Deleted = true
            },
            Errors = [],
            Message = "Competition deleted successfully"
        };

        return response;
    }
}