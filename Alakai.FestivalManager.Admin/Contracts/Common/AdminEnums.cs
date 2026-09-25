namespace Alakai.FestivalManager.Admin.Contracts.Common;

public enum DanceRole
{
    Leader = 1,
    Follower = 2,
    Individual = 3
}

public enum CompetitionFormat
{
    Individual = 1,
    Partnered = 2,
    Team = 3
}



public enum CompetitionEntryStatus
{
    Confirmed = 1,
    WaitingPartner = 2,
    Cancelled = 3
}

public enum EmailTemplateKey
{
    RegistrationCreated = 1,
    PaymentConfirmed = 2,
    PaymentFailed = 3,
    RegistrationCancelled = 4,
    WaitingPartner = 5,
    PartnerConfirmed = 6,
    CompetitionEntryConfirmed = 7,
    CompetitionEntryCancelled = 8,
    PasswordReset = 9,
    AccommodationConfirmed = 10,
    AccommodationCancelled = 11,
    BusConfirmed = 12,
    BusCancelled = 13,
    MenuConfirmed = 14,
    MenuCancelled = 15,
    AccommodationNewResponsible = 16
}

public enum EmailLogStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
    Skipped = 4
}

public enum DiscountApplicationStatus
{
    None = 0,
    PendingThreshold = 1,
    Applied = 2,
    Invalid = 3
}

public enum RegistrationStatus
{
    PendingPayment = 1,
    Registered = 2,
    Confirmed = 3,
    WaitingPartner = 4,
    Cancelled = 5
}

public enum PaymentStatus
{
    Unpaid = 1,
    Pending = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5,
    PartiallyPaid = 6
}

public enum DiscountActivationType
{
    Immediate = 1,
    AfterThreshold = 2
}

public enum DiscountType
{
    FixedAmount = 1,
    Percentage = 2
}


public enum AdminUserRole
{
    SuperAdmin = 1,
    Admin = 2,
    User = 3,
    Production = 4
}


[Flags]
public enum FestivalModule
{
    None = 0,
    Competitions = 1,
    Accommodation = 2,
    Transport = 4,
    Meals = 8
}

// Distinto de PaymentPlan (el plan elegido por cada inscripcion, valores 1/2/3
// ya grabados en produccion). Este es la mascara de que planes puede elegir
// quien se registra en un festival.
[Flags]
public enum EnabledPaymentPlan
{
    None = 0,
    FullOnline = 1,
    SplitFiftyFifty = 2,
    DeferredTenDays = 4
}

// Mirror local de Domain.Enums.EnabledPaymentPlatform (el proyecto Admin no
// referencia Domain/Application directamente).
[Flags]
public enum EnabledPaymentPlatform
{
    None = 0,
    Redsys = 1,
    Stripe = 2,
    PayPal = 4
}

public enum PaymentPlan
{
    FullOnline = 1,
    SplitFiftyFifty = 2,
    DeferredTenDays = 3
}

// Distinto de EnabledPaymentPlatform (la mascara de plataformas habilitadas
// en el festival). Este es el medio de pago realmente usado en cada
// inscripcion; mirror local de Domain.Enums.PaymentPlatform (el proyecto
// Admin no referencia Domain/Application directamente).
public enum PaymentPlatform
{
    Redsys = 1,
    Stripe = 2,
    PayPal = 3
}

public enum ProductionPersonCategory
{
    Artist = 1,
    Team = 2
}

public enum DocumentType
{
    Dni = 1,
    Passport = 2
}
