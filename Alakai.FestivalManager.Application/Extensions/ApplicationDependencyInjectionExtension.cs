namespace Alakai.FestivalManager.Application.Extensions;

public static class ApplicationDependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjectionExtension).Assembly);
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ApplicationDependencyInjectionExtension).Assembly);
        });

        //Festivals
        services.AddScoped<CreateFestivalHandler>();
        services.AddScoped<IFestivalService, FestivalService>();
        services.AddScoped<GetFestivalByIdHandler>();
        services.AddScoped<GetFestivalsHandler>();
        services.AddScoped<UpdateFestivalHandler>();
        services.AddScoped<DeleteFestivalHandler>();

        //Editions
        services.AddScoped<CreateEditionHandler>();
        services.AddScoped<GetEditionByIdHandler>();
        services.AddScoped<GetEditionsByFestivalIdHandler>();
        services.AddScoped<GetEditionsHandler>();
        services.AddScoped<UpdateEditionHandler>();
        services.AddScoped<DeleteEditionHandler>();
        services.AddScoped<IEditionService, EditionService>();

        //PassTypes
        services.AddScoped<CreatePassTypeHandler>();
        services.AddScoped<GetPassTypeByIdHandler>();
        services.AddScoped<GetPassTypesHandler>();
        services.AddScoped<GetPassTypesByEditionIdHandler>();
        services.AddScoped<UpdatePassTypeHandler>();
        services.AddScoped<DeletePassTypeHandler>();
        services.AddScoped<IPassTypeService, PassTypeService>();

        //Levels 
        services.AddScoped<CreateLevelHandler>();
        services.AddScoped<GetLevelByIdHandler>();
        services.AddScoped<GetLevelsHandler>();
        services.AddScoped<GetLevelsByPassTypeIdHandler>();
        services.AddScoped<UpdateLevelHandler>();
        services.AddScoped<DeleteLevelHandler>();
        services.AddScoped<ILevelService, LevelService>();

        //Registrations
        services.AddScoped<CreateRegistrationHandler>();
        services.AddScoped<GetRegistrationByIdHandler>();
        services.AddScoped<GetRegistrationsHandler>();
        services.AddScoped<GetRegistrationsByEditionIdHandler>();
        services.AddScoped<GetRegistrationByUserIdHandler>();
        services.AddScoped<UpdateRegistrationHandler>();
        services.AddScoped<DeleteRegistrationHandler>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IRegistrationPartnerService, RegistrationPartnerService>();

        //Users
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByEmailHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<IUserService, UserService>();

        //Competitions
        services.AddScoped<CreateCompetitionHandler>();
        services.AddScoped<GetCompetitionByIdHandler>();
        services.AddScoped<GetCompetitionsHandler>();
        services.AddScoped<GetCompetitionsByEditionIdHandler>();
        services.AddScoped<UpdateCompetitionHandler>();
        services.AddScoped<DeleteCompetitionHandler>();
        services.AddScoped<ICompetitionService, CompetitionService>();

        //CompetitionEntries
        services.AddScoped<CreateCompetitionEntryHandler>();
        services.AddScoped<GetCompetitionEntryByIdHandler>();
        services.AddScoped<GetCompetitionEntriesHandler>();
        services.AddScoped<GetCompetitionEntriesByCompetitionIdHandler>();
        services.AddScoped<GetCompetitionEntriesByRegistrationIdHandler>();
        services.AddScoped<UpdateCompetitionEntryHandler>();
        services.AddScoped<DeleteCompetitionEntryHandler>();
        services.AddScoped<ICompetitionEntryService, CompetitionEntryService>();

        //EmailTemplates
        services.AddScoped<CreateEmailTemplateHandler>();
        services.AddScoped<GetEmailTemplateByIdHandler>();
        services.AddScoped<GetEmailTemplatesHandler>();
        services.AddScoped<GetEmailTemplatesByEditionIdHandler>();
        services.AddScoped<UpdateEmailTemplateHandler>();
        services.AddScoped<DeleteEmailTemplateHandler>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IEmailLayoutService, EmailLayoutService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();

        //EmailLogs
        services.AddScoped<CreateEmailLogHandler>();
        services.AddScoped<GetEmailLogByIdHandler>();
        services.AddScoped<GetEmailLogsHandler>();
        services.AddScoped<GetEmailLogsByEditionIdHandler>();
        services.AddScoped<GetEmailLogsByRegistrationIdHandler>();
        services.AddScoped<GetEmailLogsByUserIdHandler>();
        services.AddScoped<UpdateEmailLogHandler>();
        services.AddScoped<DeleteEmailLogHandler>();
        services.AddScoped<IEmailLogService, EmailLogService>();

        //Email Rendering
        services.AddScoped<IEmailTemplateRendererService, EmailTemplateRendererService>();
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        //DiscountCodes
        services.AddScoped<CreateDiscountCodeHandler>();
        services.AddScoped<GetDiscountCodeByIdHandler>();
        services.AddScoped<GetDiscountCodesHandler>();
        services.AddScoped<GetDiscountCodesByEditionIdHandler>();
        services.AddScoped<UpdateDiscountCodeHandler>();
        services.AddScoped<DeleteDiscountCodeHandler>();
        services.AddScoped<IDiscountCalculationService, DiscountCalculationService>();
        services.AddScoped<IDiscountCodeService, DiscountCodeService>();

        //Auth
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        //User Panel
        services.AddScoped<IUserPanelService, UserPanelService>();

        return services;
    }
}
