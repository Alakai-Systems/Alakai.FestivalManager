namespace Alakai.FestivalManager.Application.Features.Users.Commands.BulkImportUsers;

public class BulkImportUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<CreateUserCommand> _rowValidator;
    private readonly IMapper _mapper;

    public BulkImportUsersHandler(IUserRepository userRepository, IValidator<CreateUserCommand> rowValidator, IMapper mapper)
    {
        _userRepository = userRepository;
        _rowValidator = rowValidator;
        _mapper = mapper;
    }

    private const int SaveBatchSize = 200;

    public async Task<BulkImportUsersResultDto> HandleAsync(BulkImportUsersCommand command, CancellationToken cancellationToken = default)
    {
        BulkImportUsersResultDto result = new() { TotalRows = command.Rows.Count };
        HashSet<string> processedEmails = new(StringComparer.OrdinalIgnoreCase);
        int pendingSinceLastSave = 0;

        for (int i = 0; i < command.Rows.Count; i++)
        {
            CreateUserCommand row = command.Rows[i];
            int rowNumber = i + 1;
            string email = (row.Email ?? string.Empty).Trim();

            ValidationResult validation = await _rowValidator.ValidateAsync(row, cancellationToken);

            if (!validation.IsValid)
            {
                result.Errors.Add(new BulkImportRowErrorDto
                {
                    Row = rowNumber,
                    Email = email,
                    Reason = string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))
                });

                continue;
            }

            string normalizedEmail = email.ToLowerInvariant();

            // Duplicado dentro del propio CSV (ya procesado en este mismo lote, todavia no
            // guardado) o ya existente en la base de datos desde antes de este import.
            bool exists = processedEmails.Contains(normalizedEmail) || await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);

            if (exists)
            {
                result.SkippedDuplicateEmails.Add(normalizedEmail);
                continue;
            }

            User user = _mapper.Map<User>(row);
            user.Email = normalizedEmail;
            user.IsActive = true;

            await _userRepository.AddAsync(user, cancellationToken);
            processedEmails.Add(normalizedEmail);
            result.Created++;
            pendingSinceLastSave++;

            if (pendingSinceLastSave >= SaveBatchSize)
            {
                await _userRepository.SaveChangesAsync(cancellationToken);
                pendingSinceLastSave = 0;
            }
        }

        if (pendingSinceLastSave > 0)
        {
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}