# Festival Modules

Modules are enabled per-festival using the `FestivalModule` flags enum. Each festival can have any combination enabled.

```csharp
[Flags]
public enum FestivalModule
{
    None         = 0,
    Competitions = 1,
    Accommodation = 2,
    Transport    = 4,
    Meals        = 8
}
```

Modules are stored on the `Festival` entity as an integer bitmask. The Admin UI and the User Panel check `HasModule(FestivalModule.X)` to show/hide sections.

---

## Core (always enabled)

### Registrations
The central module. Every festival has registrations.
- Public registration form at `/register/{slug}`
- Pass types with optional levels
- Dance roles (Leader / Follower / Individual)
- Partner linking
- Payment plans: FullOnline, SplitFiftyFifty, DeferredTenDays
- Redsys payment integration
- Discount codes with threshold or immediate activation

### Email Templates
Admin-configurable email templates with `{{variable}}` placeholders.
- Templates: RegistrationCreated, PaymentConfirmed, PaymentFailed, RegistrationCancelled, WaitingPartner, PartnerConfirmed, AccommodationConfirmed, AccommodationCancelled, AccommodationNewResponsible, BusConfirmed, BusCancelled, MenuConfirmed, MenuCancelled, PasswordReset
- Per-edition templates (falls back to global if no edition-specific one exists)
- WYSIWYG editor in Admin

### Invoices
PDF invoice generation using QuestPDF.
- Per-registration invoices
- Configurable invoice templates and issuer settings
- Auto-generated sequential invoice numbers

---

## Optional Modules

### Competitions (`FestivalModule.Competitions = 1`)
Competition management with capacity control.
- Multiple competitions per edition
- Formats: Individual / Partnered / Team
- Capacity per role (Leader / Follower / Individual)
- Threshold-based capacity with levels
- Status: WaitingPartner → Confirmed

### Accommodation (`FestivalModule.Accommodation = 2`)
Accommodation booking tied to registrations.
- Buildings with zones and individual accommodation units
- Reservation with responsible + occupants
- Occupant registration linking
- When the responsible registration is deleted, responsibility transfers automatically to another occupant
- Emails sent to all occupants on confirmation/cancellation

### Transport (`FestivalModule.Transport = 4`)
Bus reservation management.
- Outbound + return bus lines
- Per-bus capacity with pass type restrictions
- Email confirmation with departure details

### Meals (`FestivalModule.Meals = 8`)
Meal preference collection.
- Menu type selection
- Dietary requirements (celiac, gluten intolerant, allergies)
- Per-edition menu options

---

## User Panel

The User Panel (`/user-panel/`) is a self-service area for registered participants:
- Dashboard with registration and payment status
- Payment gateway (Redsys)
- Module sections gated by festival modules: Accommodation, Buses, Meals, Competitions

Access is JWT-authenticated. Users are created automatically when a registration is submitted.