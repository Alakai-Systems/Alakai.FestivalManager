namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.UpsertFestivalCredentials;

public class UpsertFestivalCredentialsHandler
{
    private readonly IFestivalRepository _festivalRepository;
    private readonly IFestivalCredentialsRepository _festivalCredentialsRepository;
    private readonly IMapper _mapper;

    public UpsertFestivalCredentialsHandler(IFestivalRepository festivalRepository, IFestivalCredentialsRepository festivalCredentialsRepository, IMapper mapper)
    {
        _festivalRepository = festivalRepository;
        _festivalCredentialsRepository = festivalCredentialsRepository;
        _mapper = mapper;
    }

    public async Task<FestivalCredentialsDto> HandleAsync(UpsertFestivalCredentialsCommand command, CancellationToken cancellationToken = default)
    {
        Festival? festival = await _festivalRepository.GetByIdAsync(command.FestivalId, cancellationToken);

        if (festival is null)
        {
            throw new NotFoundException($"Festival with id '{command.FestivalId}' was not found.");
        }

        FestivalCredentials? credentials = await _festivalCredentialsRepository.GetByFestivalIdAsync(command.FestivalId, cancellationToken);
        bool isNew = credentials is null;
        credentials ??= new FestivalCredentials { FestivalId = command.FestivalId };

        credentials.RedsysMerchantCode = command.RedsysMerchantCode;
        credentials.RedsysTerminal = command.RedsysTerminal;
        credentials.RedsysMerchantName = command.RedsysMerchantName;
        credentials.EmailHost = command.EmailHost;
        credentials.EmailPort = command.EmailPort;
        credentials.EmailUsername = command.EmailUsername;
        credentials.EmailFromEmail = command.EmailFromEmail;
        credentials.EmailFromName = command.EmailFromName;
        credentials.EmailUseSSL = command.EmailUseSSL;

        // Dejar en blanco = conservar el secreto ya guardado, no sobreescribir.
        if (!string.IsNullOrWhiteSpace(command.RedsysSecretKey))
        {
            credentials.RedsysSecretKey = command.RedsysSecretKey;
        }

        if (!string.IsNullOrWhiteSpace(command.EmailPassword))
        {
            credentials.EmailPassword = command.EmailPassword;
        }

        if (isNew)
        {
            await _festivalCredentialsRepository.AddAsync(credentials, cancellationToken);
        }
        else
        {
            credentials.SetUpdated();
            _festivalCredentialsRepository.Update(credentials);
        }

        await _festivalCredentialsRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FestivalCredentialsDto>(credentials);
    }
}