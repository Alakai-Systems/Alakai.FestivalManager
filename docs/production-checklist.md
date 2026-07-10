# Production Checklist

Everything that must be changed before deploying to production. Go through this list top to bottom.

---

## 1. API — appsettings.Production.json

Create this file in `Alakai.FestivalManager.Api/` (never commit it):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PROD-SERVER;Database=PROD-DB;User Id=PROD-USER;Password=PROD-PASS;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "<random string, minimum 32 characters, never share>"
  },
  "Redsys": {
    "MerchantCode": "<your production merchant code from bank>",
    "Terminal": "<your production terminal number>",
    "SecretKey": "<your production Redsys secret key>",
    "PaymentUrl": "https://sis.redsys.es/sis/realizarPago",
    "NotificationUrl": "https://YOUR-API-DOMAIN/api/payments/redsys/notification",
    "UrlOk": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ok",
    "UrlKo": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ko"
  },
  "Email": {
    "Host": "<production SMTP host>",
    "Port": 587,
    "UserName": "<production email>",
    "Password": "<production email password>",
    "FromEmail": "<production from email>",
    "FromName": "<festival name>"
  },
  "FileStorage": {
    "PublicBaseUrl": "https://YOUR-API-DOMAIN/uploads/email-images"
  },
  "ApplicationUrls": {
    "PortalUrl": "https://YOUR-ADMIN-DOMAIN/user-panel"
  },
  "GoogleAnalytics": {
    "CredentialsPath": "/var/secrets/ga-service-account.json"
  }
}
```

---

## 2. Admin — appsettings.Production.json

Create this file in `Alakai.FestivalManager.Admin/` (never commit it):

```json
{
  "ApiSettings": {
    "BaseUrl": "https://YOUR-API-DOMAIN/"
  },
  "ExternalAuth": {
    "GoogleClientId": "<production Google OAuth client ID>"
  }
}
```

---

## 3. Code changes required

### CORS (Api/Program.cs)
Change the hardcoded Admin origin:
```csharp
// Line ~82 — change localhost:7033 to your production Admin domain
policy.WithOrigins("https://YOUR-ADMIN-DOMAIN")
```

### Password reset URL (Application/Features/Auth/Services/AuthService.cs)
Change the hardcoded localhost URL:
```csharp
// Line ~283 — change to your production Admin domain
string resetPasswordUrl = $"https://YOUR-ADMIN-DOMAIN/user-panel/reset-password?token={encodedToken}";
```

### EmailNotificationApiClient (Admin/Extensions/ApplicationDependencyInjectionExtension.cs)
One HttpClient still has a hardcoded localhost URL (~line 192). Change to use config:
```csharp
// Change:
client.BaseAddress = new Uri("https://localhost:7157/");
// To:
client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);
```

---

## 4. Redsys — test vs production

| Setting | Test | Production |
|---|---|---|
| `PaymentUrl` | `https://sis-t.redsys.es:25443/sis/realizarPago` | `https://sis.redsys.es/sis/realizarPago` |
| `MerchantCode` | Test merchant code | Real merchant code from bank |
| `SecretKey` | Test key | Real key from bank |
| `NotificationUrl` | Any (not called in test) | Must be publicly reachable HTTPS |

The `NotificationUrl` is called server-to-server by Redsys to confirm payments. It **must** be reachable from the internet — localhost will not work.

---

## 5. Google Analytics

1. Create a Google Cloud service account with GA4 Data API access
2. Download the JSON credentials file
3. Set `GoogleAnalytics:CredentialsPath` to the path on the server
4. Set `GoogleAnalyticsPropertyId` on each Festival record in the database

---

## 6. Google OAuth (Social Login)

1. In Google Cloud Console, add your production domain to authorised redirect URIs
2. Update `ExternalAuth:Google:ClientId` in Api appsettings
3. Update `ExternalAuth:GoogleClientId` in Admin appsettings

---

## 7. Database

```bash
# Run migrations against production database
dotnet ef database update \
  --project Alakai.FestivalManager.Infrastructure \
  --startup-project Alakai.FestivalManager.Api \
  --connection "Server=PROD-SERVER;..."
```

---

## 8. File storage

The `wwwroot/uploads/` folder in the Api stores email images uploaded via the Admin. In production:
- Ensure the Api process has write access to this folder
- Consider using Azure Blob Storage or S3 for production (requires implementing `IFileStorageService`)
- Update `FileStorage:PublicBaseUrl` to point to your production API domain

---

## 9. Security checklist

- [ ] `Jwt:SecretKey` is unique per deployment, ≥ 32 characters, never committed to git
- [ ] `Email:Password` not in committed appsettings
- [ ] `Redsys:SecretKey` not in committed appsettings
- [ ] `ConnectionStrings:DefaultConnection` not in committed appsettings
- [ ] CORS `WithOrigins` contains only the production Admin domain
- [ ] Swagger UI disabled in production (already gated by `IsDevelopment()`)
- [ ] HTTPS enforced on both Api and Admin
- [ ] SQL Server user has minimum required permissions (no sa)