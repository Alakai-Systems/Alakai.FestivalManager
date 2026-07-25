# Fix-EmailImagesStripWidthHeightAttributes.ps1
#
# El metodo MakeImagesResponsive ya anadia style="max-width:100%;
# height:auto;" a las imagenes que no tenian estilo propio - pero dejaba
# los atributos HTML width/height tal cual (por ejemplo width="750"
# height="143"). Con las dos cosas a la vez, algunos motores de
# renderizado movil pueden hacer una primera pasada con el atributo HTML
# antes de que el CSS termine de aplicarse, dando un resultado
# inconsistente segun la velocidad de carga.
#
# Este cambio quita los atributos width/height de CUALQUIER <img> (tengan
# o no estilo ya puesto), dejando solo el control por CSS. Si en Outlook
# de escritorio alguna imagen se viera con un tamano raro por esto,
# revertir es tan sencillo como deshacer este mismo cambio - no toca
# nada mas del shell.
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
    private static string MakeImagesResponsive(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html;
        }

        return System.Text.RegularExpressions.Regex.Replace(
            html,
            @"<img\b(?![^>]*\bstyle\s*=)([^>]*)>",
            m => $"<img{m.Groups[1].Value} style=\"max-width:100%; height:auto;\">",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
'@ -replace "`r`n", "`n"

$new = @'
    private static string MakeImagesResponsive(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return html;
        }

        string withoutFixedSize = System.Text.RegularExpressions.Regex.Replace(
            html,
            @"<img\b([^>]*)>",
            m =>
            {
                string attributes = System.Text.RegularExpressions.Regex.Replace(m.Groups[1].Value, @"\s(width|height)\s*=\s*""[^""]*""", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return $"<img{attributes}>";
            },
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return System.Text.RegularExpressions.Regex.Replace(
            withoutFixedSize,
            @"<img\b(?![^>]*\bstyle\s*=)([^>]*)>",
            m => $"<img{m.Groups[1].Value} style=\"max-width:100%; height:auto;\">",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
'@ -replace "`r`n", "`n"

if ($normalizedContent.Contains($new) -and -not $normalizedContent.Contains($old)) {
    Write-Host "SKIP (ya aplicado)" -ForegroundColor Cyan
}
elseif ($normalizedContent.Contains($old)) {
    $updatedNormalized = $normalizedContent.Replace($old, $new)
    $updatedFinal = if ($usesCrlf) { $updatedNormalized -replace "`n", "`r`n" } else { $updatedNormalized }
    Set-Content -Path $path -Value $updatedFinal -NoNewline
    Write-Host "OK: width/height quitados de todas las imagenes, solo queda el control por CSS" -ForegroundColor Green
}
else {
    Write-Host "SKIP (anchor no encontrado - pegame el metodo MakeImagesResponsive actual completo)" -ForegroundColor Yellow
    exit 1
}

Write-Host "`nSi esto causara algun problema en Outlook de escritorio, revertir es deshacer solo este cambio - no afecta a nada mas del shell." -ForegroundColor Yellow