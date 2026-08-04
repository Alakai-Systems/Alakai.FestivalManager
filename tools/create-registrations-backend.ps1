<#
.SINOPSIS
  Dos cosas en un script:

  1) Arregla un bug mio: la funcion New-FileIfMissing que uso desde el
     Script 10 para crear ficheros nuevos duplicaba el retorno de carro
     (dejaba "\r\r\n" en vez de "\r\n") en TODOS los ficheros que creaba.
     No rompia la compilacion (C#/Razor/JS toleran la linea en blanco de
     mas), pero es un fichero mal formado y por eso el Script 17 no
     encontraba el anchor en AzureBlobStorageOptions.cs (el "\r\r\n" no
     coincidia con el texto esperado). Se arregla en estos 10 ficheros:

       - Application\Features\Tickets\Contracts\DTOs\TicketCheckInResultDto.cs
       - Application\Features\Tickets\Contracts\Requests\CheckInTicketRequest.cs
       - Api\Controllers\TicketsController.cs
       - Admin\Contracts\Tickets\DTOs\TicketCheckInResultDto.cs
       - Admin\Contracts\Tickets\Requests\CheckInTicketRequest.cs
       - Admin\Services\Api\TicketsApiClient.cs
       - Admin\wwwroot\js\checkin.js
       - Admin\Components\Pages\CheckIn.razor
       - Application\Features\Files\AzureBlobStorageOptions.cs
       - Application\Features\Files\Services\BlobFileStorageService.cs

  2) Ahora que AzureBlobStorageOptions.cs esta limpio, completa el rename
     ConnectionString -> ConnString que el Script 17 no pudo aplicar ahi
     (los otros 2 ficheros del Script 17 SI se aplicaron bien, esos no
     hace falta tocarlos otra vez).

  Idempotente (puedes relanzarlo sin problema, incluso si ya corriste el
  17 y solo fallo ese SKIP).

.USO
  Ejecutar desde la raiz del repo:
    .\18-fix-line-endings-and-finish-rename.ps1
#>

$ErrorActionPreference = "Stop"

# ============================================================================
# Repair-LineEndings: colapsa cualquier "\r"+ seguido de "\n" a un solo
# "\r\n". Si el fichero ya esta limpio, no lo toca (idempotente).
# ============================================================================
function Repair-LineEndings {
    param(
        [Parameter(Mandatory = $true)] [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)

    $fixedContent = [regex]::Replace($rawContent, "`r+`n", "`r`n")

    if ($fixedContent -eq $rawContent) {
        Write-Host "SKIP: ya estaba bien -> $Path" -ForegroundColor DarkGray
        return
    }

    [System.IO.File]::WriteAllText($Path, $fixedContent, $utf8NoBom)
    Write-Host "OK: saltos de linea arreglados -> $Path" -ForegroundColor Green
}

function Patch-File {
    param(
        [Parameter(Mandatory = $true)] [string]$Path,
        [Parameter(Mandatory = $true)] [string]$OldString,
        [Parameter(Mandatory = $true)] [string]$NewString,
        [string]$Description = ""
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        Write-Host "SKIP: archivo no encontrado -> $Path" -ForegroundColor Yellow
        return
    }

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    $rawContent = [System.IO.File]::ReadAllText($Path, [System.Text.Encoding]::UTF8)
    $usesCrlf = $rawContent.Contains("`r`n")

    $content = $rawContent -replace "`r`n", "`n"
    $oldNormalized = $OldString -replace "`r`n", "`n"
    $newNormalized = $NewString -replace "`r`n", "`n"

    if ($content.Contains($newNormalized)) {
        Write-Host "SKIP: ya aplicado -> $Path ($Description)" -ForegroundColor DarkGray
        return
    }

    $occurrences = ([regex]::Matches($content, [regex]::Escape($oldNormalized))).Count

    if ($occurrences -eq 0) {
        Write-Host "SKIP: anchor no encontrado -> $Path ($Description)" -ForegroundColor Red
        return
    }

    if ($occurrences -gt 1) {
        Write-Host "SKIP: anchor ambiguo, aparece $occurrences veces -> $Path ($Description)" -ForegroundColor Red
        return
    }

    $newContent = $content.Replace($oldNormalized, $newNormalized)

    if ($usesCrlf) {
        $newContent = $newContent -replace "`n", "`r`n"
    }

    [System.IO.File]::WriteAllText($Path, $newContent, $utf8NoBom)
    Write-Host "OK: aplicado -> $Path ($Description)" -ForegroundColor Green
}

# ----------------------------------------------------------------------------
# 1. Arreglar saltos de linea en los 10 ficheros afectados.
# ----------------------------------------------------------------------------
$FilesToRepair = @(
    ".\Alakai.FestivalManager.Application\Features\Tickets\Contracts\DTOs\TicketCheckInResultDto.cs",
    ".\Alakai.FestivalManager.Application\Features\Tickets\Contracts\Requests\CheckInTicketRequest.cs",
    ".\Alakai.FestivalManager.Api\Controllers\TicketsController.cs",
    ".\Alakai.FestivalManager.Admin\Contracts\Tickets\DTOs\TicketCheckInResultDto.cs",
    ".\Alakai.FestivalManager.Admin\Contracts\Tickets\Requests\CheckInTicketRequest.cs",
    ".\Alakai.FestivalManager.Admin\Services\Api\TicketsApiClient.cs",
    ".\Alakai.FestivalManager.Admin\wwwroot\js\checkin.js",
    ".\Alakai.FestivalManager.Admin\Components\Pages\CheckIn.razor",
    ".\Alakai.FestivalManager.Application\Features\Files\AzureBlobStorageOptions.cs",
    ".\Alakai.FestivalManager.Application\Features\Files\Services\BlobFileStorageService.cs"
)

foreach ($file in $FilesToRepair) {
    Repair-LineEndings -Path $file
}

Write-Host ""

# ----------------------------------------------------------------------------
# 2. Completar el rename ConnectionString -> ConnString en
#    AzureBlobStorageOptions.cs (ahora que esta limpio, el anchor coincide).
# ----------------------------------------------------------------------------
$OptionsPath = ".\Alakai.FestivalManager.Application\Features\Files\AzureBlobStorageOptions.cs"

Patch-File `
    -Path $OptionsPath `
    -Description "propiedad ConnectionString -> ConnString" `
    -OldString @'
    /// <summary>Connection string de la Storage Account de Azure. Vacio = Azure Blob Storage
    /// deshabilitado y se sigue usando LocalFileStorageService (disco local).</summary>
    public string ConnectionString { get; set; } = string.Empty;
'@ `
    -NewString @'
    /// <summary>Connection string de la Storage Account de Azure. Vacio = Azure Blob Storage
    /// deshabilitado y se sigue usando LocalFileStorageService (disco local).
    /// Se llama "ConnString" y no "ConnectionString" a proposito: Azure App Service
    /// bloquea cualquier Application Setting cuyo nombre termine en "ConnectionString".</summary>
    public string ConnString { get; set; } = string.Empty;
'@

Write-Host ""
Write-Host "Deberias ver 10 lineas de reparacion de saltos de linea (OK o SKIP si ya" -ForegroundColor Cyan
Write-Host "estaban bien) y 1 linea OK de aplicado para el rename." -ForegroundColor Cyan
Write-Host ""
Write-Host "SIGUIENTE PASO: dotnet build, y si compila limpio, redeploy." -ForegroundColor Cyan
Write-Host "Luego en el Portal: AzureBlobStorage__ConnString + AzureBlobStorage__ContainerName=uploads." -ForegroundColor Cyan