namespace Alakai.FestivalManager.Application.Features.Festivals.Queries.GetFestivalCredentialsByFestivalId;

public class GetFestivalCredentialsByFestivalIdHandler
{
    private readonly IFestivalCredentialsRepository _festivalCredentialsRepository;
    private readonly IMapper _mapper;

    public GetFestivalCredentialsByFestivalIdHandler(IFestivalCredentialsRepository festivalCredentialsRepository, IMapper mapper)
    {
        _festivalCredentialsRepository = festivalCredentialsRepository;
        _mapper = mapper;
    }

    public async Task<FestivalCredentialsDto?> HandleAsync(Guid festivalId, CancellationToken cancellationToken = default)
    {
        FestivalCredentials? credentials = await _festivalCredentialsRepository.GetByFestivalIdAsync(festivalId, cancellationToken);

        return credentials is null ? null : _mapper.Map<FestivalCredentialsDto>(credentials);
    }
}