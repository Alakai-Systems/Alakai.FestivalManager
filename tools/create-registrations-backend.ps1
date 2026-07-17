# Fix-Step30-AutoDetectImageHeight.ps1
#
# Anade deteccion automatica del alto real de la imagen ya subida (via un
# Image() de JS, leyendo naturalWidth/naturalHeight - no requiere CORS, solo
# lee dimensiones, no pixeles). Como el servidor ya redimensiono la imagen de
# forma proporcional al ancho elegido, el alto detectado es siempre el
# correcto - se pone como atributo HTML junto al ancho, sin riesgo de
# deformar nada. height:auto se mantiene en el CSS para que siga escalando
# bien de forma responsive en clientes modernos; el atributo HTML sirve de
# respaldo para clientes de correo que ignoran CSS.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Layout/EmailTemplateEditor.razor"

function Patch-File {
    param(
        [string]$Path,
        [string]$OldString,
        [string]$NewString,
        [string]$Description
    )

    if (-not (Test-Path $Path)) {
        Write-Host "SKIP (archivo no encontrado): $Path" -ForegroundColor Yellow
        return $false
    }

    $rawContent = Get-Content -Path $Path -Raw
    $usesCrlf = $rawContent.Contains("`r`n")

    $normalizedContent = $rawContent -replace "`r`n", "`n"
    $normalizedOld = $OldString -replace "`r`n", "`n"
    $normalizedNew = $NewString -replace "`r`n", "`n"

    if ($normalizedContent.Contains($normalizedNew) -and -not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (ya aplicado): $Description" -ForegroundColor Cyan
        return $true
    }

    if (-not $normalizedContent.Contains($normalizedOld)) {
        Write-Host "SKIP (anchor no encontrado): $Description" -ForegroundColor Yellow
        return $false
    }

    $updatedNormalized = $normalizedContent.Replace($normalizedOld, $normalizedNew)

    if ($usesCrlf) {
        $updatedFinal = $updatedNormalized -replace "`n", "`r`n"
    } else {
        $updatedFinal = $updatedNormalized
    }

    Set-Content -Path $Path -Value $updatedFinal -NoNewline
    Write-Host "OK: $Description" -ForegroundColor Green
    return $true
}

$results = @()

# --- 1. Inyectar IJSRuntime ---
$results += Patch-File -Path $path -Description "Inyectar IJSRuntime" -OldString @'
@using Radzen
@using Radzen.Blazor
@inject UploadsApiClient UploadsApiClient
'@ -NewString @'
@using Radzen
@using Radzen.Blazor
@inject UploadsApiClient UploadsApiClient
@inject IJSRuntime JsRuntime
'@

# --- 2. Anadir el script de deteccion de dimensiones, justo antes de @code ---
$results += Patch-File -Path $path -Description "Anadir script getImageDimensions" -OldString @'
        .email-rte-variable-chip:hover {
            background: #e0e7ff;
        }
</style>

@code {
'@ -NewString @'
        .email-rte-variable-chip:hover {
            background: #e0e7ff;
        }
</style>

<script>
    window.getImageDimensions = window.getImageDimensions || function (url) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve({ width: img.naturalWidth, height: img.naturalHeight });
            img.onerror = reject;
            img.src = url;
        });
    };
</script>

@code {
    private class ImageDimensions
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

'@

# --- 3. Usar la deteccion real al insertar la imagen ---
$results += Patch-File -Path $path -Description "Detectar el alto real y ponerlo como atributo junto al ancho" -OldString @'
            string url = await UploadsApiClient.UploadImageAsync(stream, e.File.Name, e.File.ContentType, UploadImageWidth);

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{url}\" width=\"{UploadImageWidth}\" style=\"max-width:100%; height:auto; display:block;\" />");
'@ -NewString @'
            string url = await UploadsApiClient.UploadImageAsync(stream, e.File.Name, e.File.ContentType, UploadImageWidth);

            int actualWidth = UploadImageWidth;
            int actualHeight = 0;

            try
            {
                ImageDimensions dimensions = await JsRuntime.InvokeAsync<ImageDimensions>("getImageDimensions", url);
                actualWidth = dimensions.Width;
                actualHeight = dimensions.Height;
            }
            catch
            {
                // Si por lo que sea no se pueden leer las dimensiones reales (red, formato raro),
                // seguimos adelante solo con el ancho elegido - height:auto en el CSS cubre el resto.
            }

            string heightAttribute = actualHeight > 0 ? $" height=\"{actualHeight}\"" : string.Empty;

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{url}\" width=\"{actualWidth}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nDeteccion automatica de alto anadida. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Ahora cada imagen subida lleva width Y height reales en el HTML, sin riesgo de deformarse." -ForegroundColor Cyan