# Configuration Reference

All configuration lives in `appsettings.json` (committed, safe defaults) and `appsettings.Production.json` (not committed, secrets).

## Api — appsettings.json

### ConnectionStrings
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;"
}
```
Standard SQL Server connection string. Uses EF Core + SQL Server provider.

### Jwt
```json
"Jwt": {
  "Issuer": "AlakaiFestivalManager",
  "Audience": "AlakaiFestivalManager",
  "SecretKey": "CHANGE_IN_PRODUCTION",
  "ExpirationMinutes": 120,
  "RefreshTokenExpirationDays": 30
}
```
- `SecretKey` must be ≥ 32 characters. Used to sign JWT tokens.
- `ExpirationMinutes` — access token lifetime.
- `RefreshTokenExpirationDays` — refresh token lifetime.

### Redsys (payment gateway)
```json
"Redsys": {
  "MerchantCode": "367830569",
  "Terminal": "2",
  "Currency": "978",
  "SecretKey": "sq7HjrUOBfKmC576ILgskD5srU870gJ7",
  "MerchantName": "Windy Hoppers Emporda",
  "PaymentUrl": "https://sis-t.redsys.es:25443/sis/realizarPago",
  "NotificationUrl": "https://YOUR-API-DOMAIN/api/payments/redsys/notification",
  "UrlOk": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ok",
  "UrlKo": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ko"
}
```
- `PaymentUrl` — test: `https://sis-t.redsys.es:25443/sis/realizarPago`, production: `https://sis.redsys.es/sis/realizarPago`
- `NotificationUrl` — must be a publicly reachable HTTPS URL (Redsys calls this server-to-server)
- `MerchantCode`, `Terminal`, `SecretKey` — provided by your bank/Redsys contract

### Email (SMTP)
```json
"Email": {
  "Host": "mail.alakai-systems.com",
  "Port": 587,
  "UserName": "info@alakai-systems.com",
  "Password": "...",
  "UseSSL": true,
  "FromEmail": "info@alakai-systems.com",
  "FromName": "Festival Manager"
}
```

### FileStorage
```json
"FileStorage": {
  "RootPath": "wwwroot/uploads/email-images",
  "PublicBaseUrl": "https://YOUR-API-DOMAIN/uploads/email-images"
}
```
Stores images uploaded via the email template editor. `PublicBaseUrl` must be the public URL of the Api.

### ExternalAuth
```json
"ExternalAuth": {
  "Google": { "ClientId": "..." },
  "Apple": { "ClientId": "" }
}
```
Google OAuth2 client ID for social login in the User Panel.

### ApplicationUrls
```json
"ApplicationUrls": {
  "PortalUrl": "https://YOUR-ADMIN-DOMAIN/user-panel"
}
```
Used in emails as the link to the User Panel.

### GoogleAnalytics
```json
"GoogleAnalytics": {
  "CredentialsPath": "/path/to/service-account.json"
}
```
Path to the Google service account JSON file for GA4 data API access.

---

## Admin — appsettings.json

### ApiSettings
```json
"ApiSettings": {
  "BaseUrl": "https://YOUR-API-DOMAIN/"
}
```
The Admin uses this to call the Api. Must include trailing slash.

### ExternalAuth
```json
"ExternalAuth": {
  "GoogleClientId": "...",
  "AppleClientId": "",
  "AppleRedirectUri": ""
}
```