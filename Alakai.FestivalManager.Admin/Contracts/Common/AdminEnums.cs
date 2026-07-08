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
    MenuCancelled = 15
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
    User = 3
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

public enum PaymentPlan
{
    FullOnline = 1,
    SplitFiftyFifty = 2,
    DeferredTenDays = 3
}
