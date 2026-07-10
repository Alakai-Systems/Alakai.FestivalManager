using System.Reflection;

namespace Alakai.FestivalManager.Tests.Unit.Application.Common;

public static class Builders
{
    private static readonly Faker _faker = new("en");

    private static T WithId<T>(T entity, Guid id) where T : class
    {
        PropertyInfo? prop = entity.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
            ?? entity.GetType().BaseType?.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        prop?.SetValue(entity, id);
        return entity;
    }

    public static Registration BuildRegistration(
        Guid? id = null,
        Guid? editionId = null,
        PaymentStatus paymentStatus = PaymentStatus.Unpaid,
        RegistrationStatus status = RegistrationStatus.PendingPayment,
        PaymentPlan paymentPlan = PaymentPlan.FullOnline,
        decimal finalPrice = 450m,
        decimal amountPaid = 0m,
        DateTime? paymentDueAt = null,
        Guid? discountCodeId = null,
        string? paymentReference = null)
    {
        Registration r = new()
        {
            EditionId = editionId ?? Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            PassTypeId = Guid.NewGuid(),
            Status = status,
            PaymentStatus = paymentStatus,
            PaymentPlan = paymentPlan,
            FinalPrice = finalPrice,
            BasePrice = finalPrice,
            AmountPaid = amountPaid,
            PaymentDueAt = paymentDueAt,
            DiscountCodeId = discountCodeId,
            PaymentReference = paymentReference,
            IsActive = true
        };
        if (id.HasValue) WithId(r, id.Value);
        return r;
    }

    public static AccommodationReservation BuildAccommodationReservation(
        Guid? responsibleRegistrationId = null,
        List<AccommodationReservationOccupant>? occupants = null)
    {
        Guid resId = Guid.NewGuid();
        Guid respId = responsibleRegistrationId ?? Guid.NewGuid();
        AccommodationReservation res = new()
        {
            EditionId = Guid.NewGuid(),
            AccommodationBuildingId = Guid.NewGuid(),
            ResponsibleRegistrationId = respId,
            Occupants = occupants ?? [new AccommodationReservationOccupant
            {
                RegistrationId = respId,
                IsResponsible = true,
                Email = _faker.Internet.Email()
            }]
        };
        WithId(res, resId);
        return res;
    }

    public static DiscountCode BuildDiscountCode(
        Guid? id = null,
        Guid? editionId = null,
        DiscountType discountType = DiscountType.Percentage,
        decimal discountValue = 10m,
        bool isActive = true,
        int currentUses = 0,
        int? activationThreshold = null,
        DiscountActivationType activationType = DiscountActivationType.Immediate,
        DateTime? startsAt = null,
        DateTime? endsAt = null)
    {
        DiscountCode code = new()
        {
            EditionId = editionId ?? Guid.NewGuid(),
            Code = _faker.Random.AlphaNumeric(8).ToUpper(),
            Name = _faker.Commerce.ProductName(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            IsActive = isActive,
            CurrentUses = currentUses,
            ActivationType = activationType,
            ActivationThreshold = activationThreshold,
            StartsAt = startsAt,
            EndsAt = endsAt
        };
        if (id.HasValue) WithId(code, id.Value);
        return code;
    }
}