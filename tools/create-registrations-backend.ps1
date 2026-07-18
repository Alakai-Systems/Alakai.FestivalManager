# Fix-Step62-RemoveImageWidthAttribute.ps1
#
# BUG REAL, misma causa exacta que la tabla del Paso 54 pero en las IMAGENES:
# Gmail Android (y Samsung Email) comprueban el ATRIBUTO HTML width en tablas
# E IMAGENES para decidir si encoger el email entero - documentado
# explicitamente para "table and img elements". El Paso 54 arreglo la tabla,
# pero las imagenes insertadas en el editor (header/footer/cuerpo) TODAVIA
# llevan width="XXX" como atributo HTML, ademas del CSS.
#
# Como el backend YA redimensiona fisicamente el archivo de la imagen al
# subirla (Paso 44, ImageSharp), quitar el atributo HTML es seguro - el
# archivo en si ya tiene el tamano correcto, el CSS max-width:100% se encarga
# del resto.
#
# IMPORTANTE: esto solo arregla las imagenes que insertes DESPUES de este fix.
# El banner que ya tienes puesto en el Header (con el width viejo ya guardado
# en la base de datos) necesita volver a insertarse - borralo del editor y
# vuelve a ponerlo desde la galeria (ya deberia estar ahi) para que se guarde
# sin el atributo width.
#
# Ejecutar DESPUES de Fix-Step45.
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

    if ($normalizedContent.Contains($normalizedNew)) {
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

# ── 1. Subida nueva: quitar width/height como atributos HTML ────────────────
$results += Patch-File -Path $path -Description "Quitar atributos width/height HTML de la imagen recien subida" -OldString @'
            UploadImageDetailResult uploaded = await UploadsApiClient.UploadImageWithDetailsAsync(
                stream, e.File.Name, e.File.ContentType, ActiveFestivalState.Active?.Id, UploadImageWidth);

            string heightAttribute = uploaded.Height > 0 ? $" height=\"{uploaded.Height}\"" : string.Empty;

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{uploaded.Url}\" width=\"{uploaded.Width}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

            await WidthChanged.InvokeAsync(UploadImageWidth);
'@ -NewString @'
            UploadImageDetailResult uploaded = await UploadsApiClient.UploadImageWithDetailsAsync(
                stream, e.File.Name, e.File.ContentType, ActiveFestivalState.Active?.Id, UploadImageWidth);

            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<img src=\"{uploaded.Url}\" style=\"max-width:100%; width:{uploaded.Width}px; height:auto; display:block;\" />");

            await WidthChanged.InvokeAsync(UploadImageWidth);
'@

# ── 2. Elegir de galeria: quitar width/height como atributos HTML ───────────
$results += Patch-File -Path $path -Description "Quitar atributos width/height HTML de la imagen elegida de galeria" -OldString @'
    private async Task InsertFromGalleryAsync(GalleryImageDto image)
    {
        string heightAttribute = image.Height > 0 ? $" height=\"{image.Height}\"" : string.Empty;

        await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
            $"<img src=\"{image.Url}\" width=\"{image.Width}\"{heightAttribute} style=\"max-width:100%; height:auto; display:block;\" />");

        ShowGallery = false;
    }
'@ -NewString @'
    private async Task InsertFromGalleryAsync(GalleryImageDto image)
    {
        await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
            $"<img src=\"{image.Url}\" style=\"max-width:100%; width:{image.Width}px; height:auto; display:block;\" />");

        ShowGallery = false;
    }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun paso no se pudo aplicar. Revisa los mensajes anteriores." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host ""
Write-Host "PASO MANUAL NECESARIO: el banner que ya tienes en el Header de Email Layout" -ForegroundColor Yellow
Write-Host "Settings fue insertado ANTES de este fix, asi que todavia tiene el atributo" -ForegroundColor Yellow
Write-Host "width viejo guardado en la base de datos. Entra al editor del Header, borra" -ForegroundColor Yellow
Write-Host "esa imagen, y vuelve a insertarla (subida nueva o desde Choose from gallery)" -ForegroundColor Yellow
Write-Host "para que se guarde ya sin el atributo width. Luego prueba con un email nuevo." -ForegroundColor Cyan