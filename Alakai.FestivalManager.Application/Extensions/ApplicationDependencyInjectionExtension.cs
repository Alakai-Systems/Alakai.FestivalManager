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
        services.AddScoped<UpsertFestivalCredentialsHandler>();
        services.AddScoped<GetFestivalCredentialsByFestivalIdHandler>();
        services.AddScoped<IFestivalCredentialsService, FestivalCredentialsService>();

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

        //ProductionBuildings
        services.AddScoped<CreateBuildingHandler>();
        services.AddScoped<GetBuildingByIdHandler>();
        services.AddScoped<GetBuildingsHandler>();
        services.AddScoped<GetBuildingsByEditionIdHandler>();
        services.AddScoped<UpdateBuildingHandler>();
        services.AddScoped<DeleteBuildingHandler>();
        services.AddScoped<IProductionAccommodationBuildingService, ProductionAccommodationBuildingService>();

        //ProductionAccommodationZones
        services.AddScoped<GetProductionAccommodationZonesHandler>();
        services.AddScoped<CreateProductionAccommodationZoneHandler>();
        services.AddScoped<GetProductionAccommodationZoneByIdHandler>();
        services.AddScoped<GetProductionAccommodationZonesByBuildingIdHandler>();
        services.AddScoped<UpdateProductionAccommodationZoneHandler>();
        services.AddScoped<DeleteProductionAccommodationZoneHandler>();
        services.AddScoped<IProductionAccommodationZoneService, ProductionAccommodationZoneService>();

        //ProductionAccommodations (la unidad/habitacion)
        services.AddScoped<GetProductionAccommodationsHandler>();
        services.AddScoped<CreateProductionAccommodationHandler>();
        services.AddScoped<GetProductionAccommodationByIdHandler>();
        services.AddScoped<GetProductionAccommodationsByZoneIdHandler>();
        services.AddScoped<UpdateProductionAccommodationHandler>();
        services.AddScoped<DeleteProductionAccommodationHandler>();
        services.AddScoped<IProductionAccommodationService, ProductionAccommodationService>();

        //ProductionTrips
        services.AddScoped<CreateTripHandler>();
        services.AddScoped<GetTripByIdHandler>();
        services.AddScoped<GetTripsByEditionIdHandler>();
        services.AddScoped<UpdateTripHandler>();
        services.AddScoped<DeleteTripHandler>();
        services.AddScoped<IProductionTripService, ProductionTripService>();

        //RunnerItineraries
        services.AddScoped<CreateItineraryHandler>();
        services.AddScoped<GetItineraryByIdHandler>();
        services.AddScoped<GetItinerariesByEditionIdHandler>();
        services.AddScoped<UpdateItineraryHandler>();
        services.AddScoped<DeleteItineraryHandler>();
        services.AddScoped<IRunnerItineraryService, RunnerItineraryService>();

        //ProductionReservations
        services.AddScoped<GetReservationsHandler>();
        services.AddScoped<CreateReservationHandler>();
        services.AddScoped<GetReservationByIdHandler>();
        services.AddScoped<GetReservationsByBuildingIdHandler>();
        services.AddScoped<UpdateReservationHandler>();
        services.AddScoped<DeleteReservationHandler>();
        services.AddScoped<IProductionReservationService, ProductionReservationService>();

        //ProductionPeople (Artistas + Equipo)
        services.AddScoped<CreateProductionPersonHandler>();
        services.AddScoped<GetProductionPersonByIdHandler>();
        services.AddScoped<GetProductionPeopleHandler>();
        services.AddScoped<GetProductionPeopleByEditionIdHandler>();
        services.AddScoped<UpdateProductionPersonHandler>();
        services.AddScoped<DeleteProductionPersonHandler>();
        services.AddScoped<IProductionPersonService, ProductionPersonService>();

        //ProductionSuppliers (Proveedores/Servicios)
        services.AddScoped<CreateProductionSupplierHandler>();
        services.AddScoped<GetProductionSupplierByIdHandler>();
        services.AddScoped<GetProductionSuppliersHandler>();
        services.AddScoped<GetProductionSuppliersByEditionIdHandler>();
        services.AddScoped<UpdateProductionSupplierHandler>();
        services.AddScoped<DeleteProductionSupplierHandler>();
        services.AddScoped<IProductionSupplierService, ProductionSupplierService>();

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
        services.AddScoped<IPublicRegistrationService, PublicRegistrationService>();

        //Users
        services.AddScoped<CreateUserHandler>();
        services.AddScoped<CreateAdminUserHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByEmailHandler>();
        services.AddScoped<UpdateUserHandler>();
        services.AddScoped<DeleteUserHandler>();
        services.AddScoped<IUserService, UserService>();

        //Accommodation
        services.AddScoped<IAccommodationBuildingService, AccommodationBuildingService>();
        services.AddScoped<IAccommodationZoneService, AccommodationZoneService>();
        services.AddScoped<IAccommodationService, AccommodationService>();
        services.AddScoped<IAccommodationReservationService, AccommodationReservationService>();
        services.AddScoped<IRegistrationFestivalInfoService, RegistrationFestivalInfoService>();
        services.AddScoped<IBusService, BusService>();
        services.AddScoped<IMealPreferenceService, MealPreferenceService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IBusReservationService, BusReservationService>();

        //Invoices
        services.AddScoped<CreateInvoiceHandler>();
        services.AddScoped<Alakai.FestivalManager.Application.Features.Invoices.Commands.UpdateInvoice.UpdateInvoiceHandler>();
        services.AddScoped<Alakai.FestivalManager.Application.Features.Invoices.Commands.DeleteInvoice.DeleteInvoiceHandler>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<UpdateInvoiceSettingsHandler>();
        services.AddScoped<IInvoiceSettingsService, InvoiceSettingsService>();
        services.AddScoped<CreateInvoiceTemplateHandler>();
        services.AddScoped<UpdateInvoiceTemplateHandler>();
        services.AddScoped<IInvoiceTemplateService, InvoiceTemplateService>();

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
