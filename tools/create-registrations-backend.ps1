# Fix-EmailShellBulletproof-v2.ps1
#
# Igual que la v1, pero con el ancla verificada contra el contenido real
# de tu archivo (pegado tal cual) - la v1 asumia que el fix de tamano de
# letra ya estaba aplicado, y no lo estaba.
#
# Mismo cambio de fondo: meta de Apple Mail (x-apple-disable-message-
# reformatting, la pieza que evita que iOS encoja el email por su
# cuenta), bloque condicional de Outlook, y tamano de letra/padding ya
# comodos siempre, sin depender del @media.
#
# Ni los Email Templates ni el Header/Footer que ya tienes creados se
# tocan - siguen entrando en los mismos huecos {headerHtml}/{bodyHtml}/
# {footerHtml} de siempre.
#
# Ejecutar desde la raiz del repo.
$ErrorActionPreference = "Stop"

$path = "Alakai.FestivalManager.Application/Features/Emails/Services/EmailNotificationService.cs"

if (-not (Test-Path $path)) {
    Write-Host "SKIP (archivo no encontrado): $path" -ForegroundColor Yellow
    exit 1
}

$rawContent = Get-Content -Path $path -Raw
$usesCrlf = $rawContent.Contains("`r`n")
$normalizedContent = $rawContent -replace "`r`n", "`n"

$old = @'
        string wrappedHtml = $@"<!DOCTYPE html>
        <html lang=""es"">
        <head>
          <meta charset=""UTF-8"" />
          <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
          <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
          <title></title>
          <style>
            body {{ margin:0; padding:0; }}
            .email-shell img {{ max-width:100% !important; height:auto !important; }}
            @media only screen and (max-width: 600px) {{
              .email-shell {{ width:100% !important; }}
              .email-body-cell {{ padding:18px !important; font-size:16px !important; line-height:1.5 !important; }}
              .email-footer-cell {{ padding:16px 18px !important; font-size:13px !important; }}
              .email-body-cell img, .email-header-cell img, .email-footer-cell img {{ height:auto !important; }}
            }}
          </style>
        </head>
        <body style=""margin:0; padding:0;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f4f6; margin:0; padding:24px 0;"">
          <tr>
            <td align=""center"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""email-shell"" style=""width:100%; max-width:{EmailShellWidth}px; background:#ffffff;"">
                <tr>
                  <td class=""email-header-cell"" style=""overflow:auto;"">{headerHtml}</td>
                </tr>
                <tr>
                  <td class=""email-body-cell"" style=""padding:24px; font-family:Arial,Helvetica,sans-serif; font-size:14px; color:#111827;"">{bodyHtml}</td>
                </tr>
                <tr>
                  <td style=""padding:0 24px;""><hr style=""border:none; border-top:1px solid #e5e7eb; margin:0;"" /></td>
                </tr>
                <tr>
                  <td class=""email-footer-cell"" style=""overflow:auto; padding:20px 24px; font-family:Arial,Helvetica,sans-serif; font-size:12px; color:#6b7280;"">{footerHtml}</td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
        </body>
        </html>";
'@ -replace "`r`n", "`n"

$new = @'
        string wrappedHtml = $@"<!DOCTYPE html>
        <html lang=""es"">
        <head>
          <meta charset=""UTF-8"" />
          <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
          <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
          <meta name=""x-apple-disable-message-reformatting"" />
          <meta name=""format-detection"" content=""telephone=no, date=no, address=no, email=no"" />
          <title></title>
          <!--[if mso]>
          <noscript>
          <xml>
            <o:OfficeDocumentSettings>
              <o:PixelsPerInch>96</o:PixelsPerInch>
            </o:OfficeDocumentSettings>
          </xml>
          </noscript>
          <![endif]-->
          <style>
            body {{ margin:0; padding:0; width:100% !important; }}
            .email-shell img {{ max-width:100% !important; height:auto !important; }}
            @media only screen and (max-width: 600px) {{
              .email-shell {{ width:100% !important; }}
            }}
          </style>
        </head>
        <body style=""margin:0; padding:0; width:100% !important;"">
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f3f4f6; margin:0; padding:24px 0;"">
          <tr>
            <td align=""center"">
              <!--[if mso]>
              <table role=""presentation"" align=""center"" width=""{EmailShellWidth}"" cellpadding=""0"" cellspacing=""0"" style=""width:{EmailShellWidth}px;"">
              <tr>
              <td>
              <![endif]-->
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" class=""email-shell"" style=""width:100%; max-width:{EmailShellWidth}px; background:#ffffff; margin:0 auto;"">
                <tr>
                  <td class=""email-header-cell"" style=""overflow:auto;"">{headerHtml}</td>
                </tr>
                <tr>
                  <td class=""email-body-cell"" style=""padding:20px; font-family:Arial,Helvetica,sans-serif; font-size:16px; line-height:1.5; color:#111827;"">{bodyHtml}</td>
                </tr>
                <tr>
                  <td style=""padding:0 24px;""><hr style=""border:none; border-top:1px solid #e5e7eb; margin:0;"" /></td>
                </tr>
                <tr>
                  <td class=""email-footer-cell"" style=""overflow:auto; padding:16px 20px; font-family:Arial,Helvetica,sans-serif; font-size:13px; color:#6b7280;"">{footerHtml}</td>
                </tr>
              </table>
              <!--[if mso]>
              </td>
              </tr>
              </table>
              <![endif]-->
            </td>
          </tr>
        </table>
        </body>
        </html>";
'@ -replace "`r`n", "`n"

if ($normalizedContent.Contains($new) -and -not $normalizedContent.Contains($old)) {
    Write-Host "SKIP (ya aplicado)" -ForegroundColor Cyan
}
elseif ($normalizedContent.Contains($old)) {
    $updatedNormalized = $normalizedContent.Replace($old, $new)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $path -Value $updatedFinal -NoNewline
    Write-Host "OK: shell del email reescrito con patron fluid-hybrid + Outlook + Apple Mail" -ForegroundColor Green
}
else {
    Write-Host "SKIP (anchor no encontrado - algo distinto de lo pegado, revisa a mano)" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nNi los Email Templates ni el Header/Footer que ya tienes creados se han tocado." -ForegroundColor Green