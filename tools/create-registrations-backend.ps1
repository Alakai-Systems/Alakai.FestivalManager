# Fix-Step83-DataProtectionPersistence.ps1
#
# BUG REAL, y de fondo (no especifico de la impersonacion): la app no tenia
# configurado donde guardar las claves de cifrado de Data Protection (las
# que usa ProtectedLocalStorage para el token del participante). Sin
# configuracion explicita, ASP.NET Core usa una resolucion por defecto que
# puede fallar entre reinicios/recompilaciones frecuentes de la app - la
# clave que cifro un valor deja de estar disponible para descifrarlo despues,
# dando exactamente el error "The key {...} was not found in the key ring."
#
# Esto afecta a CUALQUIER login normal tambien (no solo a la impersonacion),
# simplemente se ha hecho visible ahora por las muchas recompilaciones
# seguidas de esta sesion. Fix: persistir las claves en una carpeta fija del
# disco, para que sobrevivan entre reinicios.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Program.cs"

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

$result = Patch-File -Path $path -Description "Persistir las claves de Data Protection en una carpeta fija" -OldString @'
builder.Services.AddCascadingAuthenticationState();
'@ -NewString @'
string dataProtectionKeysPath = builder.Configuration["DataProtection:KeyRingPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "keys");

Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("AlakaiFestivalManagerAdmin");

builder.Services.AddCascadingAuthenticationState();
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de Program.cs (Admin) alrededor de AddCascadingAuthenticationState." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host ""
Write-Host "IMPORTANTE: se crea una carpeta 'keys' junto al ejecutable del Admin (o donde" -ForegroundColor Yellow
Write-Host "indiques con la App Setting DataProtection__KeyRingPath en Azure). Anadela a" -ForegroundColor Yellow
Write-Host ".gitignore si no quieres que esas claves entren al repo." -ForegroundColor Yellow
Write-Host ""
Write-Host "Tras aplicar esto, cierra la app, borra la carpeta 'keys' si ya se creo antes" -ForegroundColor Cyan
Write-Host "de este fix (para partir de cero), y vuelve a probar login e impersonacion." -ForegroundColor Cyan