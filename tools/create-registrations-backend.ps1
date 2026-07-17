# Fix-Step28-ResizableUploadedImage.ps1
# Al insertar la imagen subida via InsertHtml generico, no queda con asas de
# redimensionar (a diferencia de las imagenes insertadas por el icono nativo
# de Radzen, que ya quitamos por romper Gmail). Se envuelve en un contenedor
# con "resize: both" de CSS - soporte nativo del navegador para arrastrar y
# redimensionar, sin depender de ninguna API interna de Radzen.
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

$result = Patch-File -Path $path -Description "Insertar la imagen dentro de un contenedor redimensionable (resize:both)" -OldString @'
            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml, $"<img src=\"{url}\" width=\"{UploadImageWidth}\" style=\"max-width:100%;\" />");
'@ -NewString @'
            await EditorRef.ExecuteCommandAsync(HtmlEditorCommands.InsertHtml,
                $"<span contenteditable=\"false\" style=\"display:inline-block; resize:both; overflow:hidden; max-width:100%; border:1px dashed transparent;\">" +
                $"<img src=\"{url}\" width=\"{UploadImageWidth}\" style=\"width:100%; height:auto; display:block;\" /></span>&nbsp;");
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de OnImageSelectedAsync en EmailTemplateEditor.razor." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Tras subir una imagen, arrastra la esquina inferior derecha del recuadro para redimensionarla." -ForegroundColor Cyan
Write-Host "IMPORTANTE: ese span envoltorio es solo para editar comodamente - el 'width' del <img>" -ForegroundColor Yellow
Write-Host "es lo que de verdad importa para el email final (Gmail no respeta CSS resize)." -ForegroundColor Yellow