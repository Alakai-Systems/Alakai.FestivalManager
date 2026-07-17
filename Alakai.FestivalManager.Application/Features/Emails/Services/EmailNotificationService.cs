using Alakai.FestivalManager.Infrastructure.Email;

namespace Alakai.FestivalManager.Application.Features.Emails.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IEmailTemplateRepository _emailTemplateRepository;
    private readonly IEmailLogRepository _emailLogRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailTemplateRendererService _emailTemplateRendererService;
    private readonly IEmailSender _emailSender;
    private readonly IEmailLayoutRepository _emailLayoutRepository;
    private readonly IAccommodationReservationRepository _accommodationReservationRepository;
    private readonly IBusReservationRepository _busReservationRepository;
    private readonly IMealPreferenceRepository _mealPreferenceRepository;
    private readonly IAccommodationBuildingRepository _accommodationBuildingRepository;
    private readonly ICompetitionEntryRepository _competitionEntryRepository;
    private readonly IMapper _mapper;
    private readonly SystemEmailOptions _systemEmailOptions;

    public EmailNotificationService(IEmailTemplateRepository emailTemplateRepository, IEmailLogRepository emailLogRepository, 
        IEmailTemplateRendererService emailTemplateRendererService, IMapper mapper, IRegistrationRepository registrationRepository,
        IEmailSender emailSender, IUserRepository userRepository, IEmailLayoutRepository emailLayoutRepository,
        IAccommodationReservationRepository accommodationReservationRepository, IBusReservationRepository busReservationRepository,
        IMealPreferenceRepository mealPreferenceRepository, IAccommodationBuildingRepository accommodationBuildingRepository,
        IOptions<SystemEmailOptions> systemEmailOptions, ICompetitionEntryRepository competitionEntryRepository)
    {
        _emailTemplateRepository = emailTemplateRepository;
        _emailLogRepository = emailLogRepository;
        _emailTemplateRendererService = emailTemplateRendererService;
        _mapper = mapper;
        _registrationRepository = registrationRepository;
        _emailSender = emailSender;
        _userRepository = userRepository;
        _emailLayoutRepository = emailLayoutRepository;
        _accommodationReservationRepository = accommodationReservationRepository;
        _busReservationRepository = busReservationRepository;
        _mealPreferenceRepository = mealPreferenceRepository;
        _accommodationBuildingRepository = accommodationBuildingRepository;
        _systemEmailOptions = systemEmailOptions.Value;
        _competitionEntryRepository = competitionEntryRepository;
    }

    private const int EmailShellWidth = 640;

    private async Task<(string Html, string? Text)> ApplyLayoutAsync(string bodyHtml, string? bodyText, Guid? editionId, CancellationToken cancellationToken)
    {
        EmailLayout? layout = await _emailLayoutRepository.GetForEditionAsync(editionId, cancellationToken);

        string headerHtml = layout?.HeaderHtml ?? string.Empty;
        string footerHtml = layout?.FooterHtml ?? string.Empty;

        string wrappedHtml = $@"
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f4f6; margin:0; padding:24px 0;"">
          <tr>
            <td align=""center"">
              <table role=""presentation"" width=""{EmailShellWidth}"" cellpadding=""0"" cellspacing=""0"" style=""width:{EmailShellWidth}px; max-width:100%; background:#ffffff;"">
                <tr>
                  <td style=""overflow:auto;"">{headerHtml}</td>
                </tr>
                <tr>
                  <td style=""padding:24px; font-family:Arial,Helvetica,sans-serif; font-size:14px; color:#111827;"">{bodyHtml}</td>
                </tr>
                <tr>
                  <td style=""padding:0 24px;""><hr style=""border:none; border-top:1px solid #e5e7eb; margin:0;"" /></td>
                </tr>
                <tr>
                  <td style=""overflow:auto; padding:20px 24px; font-family:Arial,Helvetica,sans-serif; font-size:12px; color:#6b7280;"">{footerHtml}</td>
                </tr>
              </table>
            </td>
          </tr>
        </table>";

        string? wrappedText = bodyText is null
            ? null
            : $"{layout?.HeaderText}{Environment.NewLine}{bodyText}{Environment.NewLine}{layout?.FooterText}";

        return (wrappedHtml, wrappedText);
    }

    public async Task<EmailLogDto?> CreateEmailLogAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default)
    {
        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);

        if (registration is null)
        {
            return null;
        }

        EmailTemplate? template = await _emailTemplateRepository.GetByKeyAndLanguageAsync(templateKey, registration.EditionId, registration.Language, cancellationToken);

        if (template is null)
        {
            return null;
        }

        Dictionary<string, string> variables = new()
        {
            ["FirstName"] = registration.FirstName,
            ["LastName"] = registration.LastName,
            ["FullName"] = $"{registration.FirstName} {registration.LastName}",
            ["Email"] = registration.Email,
            ["EditionName"] = registration.Edition.Name,
            ["EditionYear"] = registration.Edition.Year.ToString(),
            ["PassTypeName"] = registration.PassType.Name,
            ["LevelName"] = registration.Level?.Name ?? string.Empty,
            ["FinalPrice"] = registration.FinalPrice.ToString("0.00"),
            ["PartnerEmail"] = registration.PartnerEmail ?? string.Empty,
            ["RegistrationId"] = registration.Id.ToString(),
            ["FestivalName"] = registration.Edition.Name ?? string.Empty,
            ["PanelUser"] = registration.Email,
            ["PortalUrl"] = "https://lajambarcelona.com/account",
        };

        await AddAccommodationVariablesAsync(variables, registration.Id, cancellationToken);
        await AddBusVariablesAsync(variables, registration.Id, cancellationToken);
        await AddMealVariablesAsync(variables, registration.Id, cancellationToken);
        await AddCompetitionVariablesAsync(variables, registration.Id, cancellationToken);

        string subject = _emailTemplateRendererService.Render(template.Subject, variables);
        string renderedBodyHtml = _emailTemplateRendererService.Render(template.BodyHtml, variables);
        string? renderedBodyText = string.IsNullOrWhiteSpace(template.BodyText)
            ? null
            : _emailTemplateRendererService.Render(template.BodyText, variables);

        (string bodyHtml, string? bodyText) = await ApplyLayoutAsync(renderedBodyHtml, renderedBodyText, registration.EditionId, cancellationToken);


        EmailLog emailLog = new()
        {
            EditionId = registration.EditionId,
            EmailTemplateId = template.Id,
            RegistrationId = registration.Id,
            UserId = registration.UserId,
            TemplateKey = template.TemplateKey,
            RecipientEmail = registration.Email,
            RecipientName = $"{registration.FirstName} {registration.LastName}",
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Status = EmailLogStatus.Pending,
            IsActive = true
        };

        await _emailLogRepository.AddAsync(emailLog, cancellationToken);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        EmailLogDto dto = _mapper.Map<EmailLogDto>(emailLog);

        return dto;
    }


    private async Task AddCompetitionVariablesAsync(Dictionary<string, string> variables, Guid registrationId, CancellationToken cancellationToken)
    {
        variables["CompetitionName"] = string.Empty;

        IReadOnlyList<CompetitionEntry> entries = await _competitionEntryRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        CompetitionEntry? entry = entries.FirstOrDefault(e => e.IsActive) ?? entries.FirstOrDefault();

        if (entry?.Competition is not null)
        {
            variables["CompetitionName"] = entry.Competition.Name;
        }
    }

    private async Task AddAccommodationVariablesAsync(Dictionary<string, string> variables, Guid registrationId, CancellationToken cancellationToken)
    {
        variables["AccommodationBuildingName"] = string.Empty;
        variables["AccommodationOccupantNames"] = string.Empty;

        AccommodationReservation? reservation = await _accommodationReservationRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        if (reservation is null)
        {
            return;
        }

        string buildingName = reservation.AccommodationBuilding?.Name ?? string.Empty;
        if (string.IsNullOrEmpty(buildingName))
        {
            AccommodationBuilding? building = await _accommodationBuildingRepository.GetByIdAsync(reservation.AccommodationBuildingId, cancellationToken);
            buildingName = building?.Name ?? string.Empty;
        }
        variables["AccommodationBuildingName"] = buildingName;
        variables["AccommodationOccupantNames"] = string.Join(", ", reservation.Occupants.Select(o =>
            o.Registration is not null ? $"{o.Registration.FirstName} {o.Registration.LastName}" : o.Email));
    }

    private async Task AddBusVariablesAsync(Dictionary<string, string> variables, Guid registrationId, CancellationToken cancellationToken)
    {
        variables["DepartureBusTime"] = string.Empty;
        variables["DepartureBusPickup"] = string.Empty;
        variables["DepartureBusDestination"] = string.Empty;
        variables["DepartureBusPrice"] = string.Empty;
        variables["DepartureBusLine"] = string.Empty;
        variables["ReturnBusTime"] = string.Empty;
        variables["ReturnBusPickup"] = string.Empty;
        variables["ReturnBusDestination"] = string.Empty;
        variables["ReturnBusPrice"] = string.Empty;
        variables["ReturnBusLine"] = string.Empty;

        IReadOnlyList<BusReservation> reservations = await _busReservationRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        BusReservation? departure = reservations.FirstOrDefault(r => r.Bus?.Direction == BusDirection.Outbound);
        BusReservation? returnTrip = reservations.FirstOrDefault(r => r.Bus?.Direction == BusDirection.Return);

        if (departure?.Bus is not null)
        {
            variables["DepartureBusTime"] = departure.Bus.DepartureTime.ToString("dd/MM/yyyy HH:mm");
            variables["DepartureBusPickup"] = departure.Bus.PickupLocation;
            variables["DepartureBusDestination"] = departure.Bus.DestinationLocation;
            variables["DepartureBusPrice"] = departure.Bus.Price.ToString("0.00");
            variables["DepartureBusLine"] = $"{departure.Bus.DepartureTime.ToString("dd/MM/yyyy HH:mm")} - {departure.Bus.PickupLocation} - {departure.Bus.DestinationLocation} - {departure.Bus.Price.ToString("0.00")}";
        }

        if (returnTrip?.Bus is not null)
        {
            variables["ReturnBusTime"] = returnTrip.Bus.DepartureTime.ToString("dd/MM/yyyy HH:mm");
            variables["ReturnBusPickup"] = returnTrip.Bus.PickupLocation;
            variables["ReturnBusDestination"] = returnTrip.Bus.DestinationLocation;
            variables["ReturnBusPrice"] = returnTrip.Bus.Price.ToString("0.00");
            variables["ReturnBusLine"] = $"{returnTrip.Bus.DepartureTime.ToString("dd/MM/yyyy HH:mm")} - {returnTrip.Bus.PickupLocation} - {returnTrip.Bus.DestinationLocation} - {returnTrip.Bus.Price.ToString("0.00")}";
        }
    }

    private async Task AddMealVariablesAsync(Dictionary<string, string> variables, Guid registrationId, CancellationToken cancellationToken)
    {
        variables["MenuType"] = string.Empty;
        variables["IsCeliacOrGlutenIntolerant"] = string.Empty;
        variables["AllergiesNotes"] = string.Empty;

        MealPreference? preference = await _mealPreferenceRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

        if (preference is null)
        {
            return;
        }

        variables["MenuType"] = preference.MenuType.ToString();
        variables["IsCeliacOrGlutenIntolerant"] = preference.IsCeliacOrGlutenIntolerant ? "Yes" : "No";
        variables["AllergiesNotes"] = preference.AllergiesNotes ?? string.Empty;
    }

    public async Task<EmailLogDto?> CreateAndSendEmailAsync(EmailTemplateKey templateKey, Guid registrationId, CancellationToken cancellationToken = default)
    {
        EmailLogDto? emailLogDto = await CreateEmailLogAsync(templateKey, registrationId, cancellationToken);

        if (emailLogDto is null)
        {
            return null;
        }

        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(emailLogDto.Id, cancellationToken);

        if (emailLog is null)
        {
            return null;
        }

        Registration? registration = await _registrationRepository.GetByIdAsync(registrationId, cancellationToken);
        FestivalCredentials? credentials = registration?.Edition?.Festival?.Credentials;

        if (credentials is null)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = "Email configuration missing for this festival.";
            _emailLogRepository.Update(emailLog);
            await _emailLogRepository.SaveChangesAsync(cancellationToken);

            return _mapper.Map<EmailLogDto>(emailLog);
        }

        EmailSenderSettings senderSettings = new()
        {
            Host = credentials.EmailHost,
            Port = credentials.EmailPort,
            UserName = credentials.EmailUsername,
            Password = credentials.EmailPassword,
            UseSSL = credentials.EmailUseSSL,
            FromEmail = credentials.EmailFromEmail,
            FromName = credentials.EmailFromName
        };

        try
        {
            EmailMessage message = new()
            {
                To = new EmailAddress
                {
                    Name = emailLog.RecipientName ?? string.Empty,
                    Address = emailLog.RecipientEmail
                },
                Subject = emailLog.Subject,
                HtmlBody = emailLog.BodyHtml,
                TextBody = emailLog.BodyText ?? string.Empty
            };

            await _emailSender.SendAsync(message, senderSettings, cancellationToken);

            emailLog.Status = EmailLogStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            emailLog.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
        }

        _emailLogRepository.Update(emailLog);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailLogDto>(emailLog);
    }

    public async Task<EmailLogDto?> CreateAndSendPasswordResetEmailAsync(Guid userId, string resetPasswordUrl, CancellationToken cancellationToken = default)
    {
        User? user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        EmailTemplate? template = await _emailTemplateRepository.GetByKeyAsync(EmailTemplateKey.PasswordReset, null, cancellationToken);

        if (template is null)
        {
            return null;
        }

        Dictionary<string, string> variables = new()
        {
            ["FirstName"] = user.FirstName,
            ["LastName"] = user.LastName,
            ["FullName"] = $"{user.FirstName} {user.LastName}",
            ["Email"] = user.Email,
            ["ResetPasswordUrl"] = resetPasswordUrl
        };

        string subject = _emailTemplateRendererService.Render(template.Subject, variables);
        string renderedBodyHtml = _emailTemplateRendererService.Render(template.BodyHtml, variables);
        string? renderedBodyText = string.IsNullOrWhiteSpace(template.BodyText) ? null : _emailTemplateRendererService.Render(template.BodyText, variables);

        (string bodyHtml, string? bodyText) = await ApplyLayoutAsync(renderedBodyHtml, renderedBodyText, null, cancellationToken);

        EmailLog emailLog = new()
        {
            UserId = user.Id,
            TemplateKey = template.TemplateKey,
            EmailTemplateId = template.Id,
            RecipientEmail = user.Email,
            RecipientName = $"{user.FirstName} {user.LastName}",
            Subject = subject,
            BodyHtml = bodyHtml,
            BodyText = bodyText,
            Status = EmailLogStatus.Pending,
            IsActive = true
        };

        await _emailLogRepository.AddAsync(emailLog, cancellationToken);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        EmailSenderSettings systemSenderSettings = new()
        {
            Host = _systemEmailOptions.Host,
            Port = _systemEmailOptions.Port,
            UserName = _systemEmailOptions.UserName,
            Password = _systemEmailOptions.Password,
            UseSSL = _systemEmailOptions.UseSSL,
            FromEmail = _systemEmailOptions.FromEmail,
            FromName = _systemEmailOptions.FromName
        };

        return await SendExistingEmailLogAsync(emailLog.Id, systemSenderSettings, cancellationToken);
    }

    private async Task<EmailLogDto?> SendExistingEmailLogAsync(Guid emailLogId, EmailSenderSettings senderSettings, CancellationToken cancellationToken = default)
    {
        EmailLog? emailLog = await _emailLogRepository.GetByIdAsync(emailLogId, cancellationToken);

        if (emailLog is null)
        {
            return null;
        }

        try
        {
            EmailMessage message = new()
            {
                To = new EmailAddress
                {
                    Name = emailLog.RecipientName,
                    Address = emailLog.RecipientEmail
                },
                Subject = emailLog.Subject,
                HtmlBody = emailLog.BodyHtml,
                TextBody = emailLog.BodyText ?? string.Empty
            };

            await _emailSender.SendAsync(message, senderSettings, cancellationToken);

            emailLog.Status = EmailLogStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
            emailLog.ErrorMessage = null;
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailLogStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
        }

        _emailLogRepository.Update(emailLog);
        await _emailLogRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<EmailLogDto>(emailLog);
    }
}