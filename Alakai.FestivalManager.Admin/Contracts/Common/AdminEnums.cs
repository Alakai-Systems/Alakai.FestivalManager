namespace Alakai.FestivalManager.Admin.Contracts.Common;

public enum DanceRole
{
    Leader = 1,
    Follower = 2,
    Individual = 3
}

public enum CompetitionFormat
{
    SoloJazz = 1,
    MixAndMatch = 2,
    Strictly = 3
}


public enum MixAndMatchLevel
{
    Open = 1,
    Advanced = 2
}

public enum CompetitionEntryStatus
{
    Registered = 1,
    Confirmed = 2,
    WaitingPartner = 3,
    Cancelled = 4
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
    PasswordReset = 9
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
    Pending = 1,
    Unpaid = 2,
    Paid = 3,
    Failed = 4,
    Refunded = 5
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