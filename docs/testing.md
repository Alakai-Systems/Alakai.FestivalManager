# Testing

## Stack

| Package | Version | Purpose |
|---|---|---|
| xUnit | 2.9.3 | Test runner |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 8.3.1 | Readable assertions |
| Bogus | 35.6.3 | Test data generation |

## Running tests

```bash
# All tests
dotnet test Alakai.FestivalManager.Tests

# With coverage
dotnet test Alakai.FestivalManager.Tests --collect:"XPlat Code Coverage"

# Specific test class
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

## Test structure

```
Tests/
  Unit/
    Application/
      Common/
        Builders.cs                    ← shared test data builders (Bogus + reflection for protected Id)
      Features/
        Auth/
          AuthServiceLoginTests.cs     ← login, lockout, failed attempts
          JwtServiceTests.cs           ← token generation, claims, refresh tokens
        CompetitionEntries/
          CreateCompetitionEntryHandlerTests.cs
        DiscountCodes/
          DiscountCalculationServiceTests.cs
        Emails/
          EmailTemplateRendererServiceTests.cs
        Payments/
          PaymentServiceTests.cs       ← split payment flow, auth code storage
        Registrations/
          CreateRegistrationHandlerTests.cs
          DeleteRegistrationHandlerTests.cs
          UpdateRegistrationHandlerTests.cs
      Validators/
        CreateRegistrationCommandValidatorTests.cs
        CreateDiscountCodeCommandValidatorTests.cs
        CreateLevelValidatorTests.cs
        CreateEditionValidatorTests.cs
        CreateCompetitionCommandValidatorTests.cs
        LoginCommandValidatorTests.cs
  Integration/
    Api/
      Controllers/
        README.md                      ← integration tests not yet implemented
```

## Coverage areas

| Area | Tests | Key scenarios |
|---|---|---|
| `EmailTemplateRendererService` | 11 | Substitution, null values, HTML tag stripping in placeholders, whitespace |
| `DiscountCalculationService` | 10 | Percentage/fixed discounts, expired/inactive codes, threshold activation |
| `PaymentService` | 13 | CreateSession, split payment guard, auth codes, declined payments |
| `DeleteRegistrationHandler` | 10 | Cascade cleanup, accommodation responsibility transfer, partner nulling |
| `AuthService` (login) | 8 | User not found, inactive, locked, lockout expiry, failed attempt counter, email normalisation |
| `JwtService` | 9 | Token generation, claims, refresh token uniqueness, invalid token handling |
| `CreateRegistrationHandler` | 9 | Not found, duplicate email, level validation, payment due dates, user creation |
| `UpdateRegistrationHandler` | 6 | Not found, Paid→Confirmed status, duplicate email on change |
| `CreateCompetitionEntryHandler` | 9 | Not found, duplicate entry, capacity full, unlimited capacity, partner/no-partner status |
| Validators | 48 | Required fields, length limits, email format, date ordering, enum values |

## Builders

`Builders.cs` provides factory methods for domain entities. Because `BaseEntity.Id` has a `protected set`, the builders use reflection to assign a specific Id when needed:

```csharp
Registration reg = Builders.BuildRegistration(
    id: Guid.NewGuid(),
    paymentStatus: PaymentStatus.PartiallyPaid,
    paymentPlan: PaymentPlan.SplitFiftyFifty,
    finalPrice: 450m,
    amountPaid: 225m);
```

## What is not tested

- **Controllers** — thin delegation only, no business logic
- **Simple CRUD handlers** (GetAll, GetById, Delete without logic) — no branching
- **Services that delegate entirely** to repositories (FestivalService, EditionService, etc.)
- **Integration tests** — require a test database; planned for future implementation