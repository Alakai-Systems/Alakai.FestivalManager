# API Reference

The API is a standard ASP.NET Core Web API with JWT Bearer authentication.

## Base URL

- Development: `https://localhost:7157`
- Production: configured per deployment

## Swagger

Available at `/swagger` in development only.

## Authentication

All endpoints except public ones require a JWT Bearer token:

```
Authorization: Bearer <token>
```

Tokens are obtained via `POST /api/auth/login`.

### Public endpoints (no auth required)
- `GET /api/public-festivals` — list active festivals for public registration form
- `GET /api/public-registrations/{slug}` — festival info for registration form
- `POST /api/public-registrations` — submit a registration
- `POST /api/auth/login` — obtain JWT token
- `POST /api/auth/refresh-token` — refresh access token
- `POST /api/auth/forgot-password` — request password reset
- `POST /api/auth/reset-password` — submit new password
- `POST /api/payments/redsys/return` — Redsys payment return URL
- `POST /api/payments/redsys/notification` — Redsys server-to-server notification

## Response format

All responses follow `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "errors": [],
  "message": "..."
}
```

Errors return RFC 7807 Problem Details via `GlobalExceptionMiddleware`:

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 |
| `BusinessRuleException` | 422 |
| `ValidationException` | 400 |
| Unhandled | 500 |

## Main endpoint groups

| Prefix | Description |
|---|---|
| `/api/festivals` | Festival CRUD |
| `/api/editions` | Edition CRUD |
| `/api/pass-types` | Pass type CRUD |
| `/api/levels` | Level CRUD |
| `/api/registrations` | Registration CRUD + status management |
| `/api/payments` | Redsys payment session creation and callbacks |
| `/api/competitions` | Competition CRUD |
| `/api/competition-entries` | Competition entry management |
| `/api/accommodations` | Accommodation unit management |
| `/api/accommodation-buildings` | Building + zone management |
| `/api/accommodation-reservations` | Reservation CRUD |
| `/api/buses` | Bus line management |
| `/api/bus-reservations` | Bus booking |
| `/api/meal-preferences` | Meal preference management |
| `/api/email-templates` | Email template CRUD |
| `/api/emails` | Manual email sending |
| `/api/email-logs` | Sent email history |
| `/api/invoices` | Invoice generation and retrieval |
| `/api/discount-codes` | Discount code management |
| `/api/reports` | Excel report exports |
| `/api/dashboard` | KPI stats and revenue data |
| `/api/analytics` | Google Analytics 4 data |
| `/api/users` | User management |
| `/api/auth` | Authentication |
| `/api/user-panel` | User Panel specific endpoints |
| `/api/uploads` | Image upload for email templates |