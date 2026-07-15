# fix_admin_program.ps1
# Añade ForwardedHeaders para que AntiForgery funcione detrás del proxy de Azure
# Ejecutar desde la raiz del repo: .\tools\fix_admin_program.ps1

$ErrorActionPreference = "Stop"

$file = "Alakai.FestivalManager.Admin\Program.cs"

if (-not (Test-Path $file)) {
    Write-Error "ABORT: No se encuentra $file"
    exit 1
}

function Patch-File {
    param([string]$Path, [string]$OldText, [string]$NewText, [string]$Description)
    $raw = [System.IO.File]::ReadAllText($Path)
    $rawNorm = $raw.Replace("`r`n", "`n")
    $oldNorm = $OldText.Replace("`r`n", "`n")
    $newNorm = $NewText.Replace("`r`n", "`n")
    $count = ([regex]::Matches($rawNorm, [regex]::Escape($oldNorm))).Count
    if ($count -ne 1) {
        Write-Error "ABORT: '$Description' — esperaba 1 en '$Path', encontradas $count"
        exit 1
    }
    $useCRLF = $raw.Contains("`r`n")
    $patched = $rawNorm.Replace($oldNorm, $newNorm)
    if ($useCRLF) { $patched = $patched.Replace("`n", "`r`n") }
    [System.IO.File]::WriteAllText($Path, $patched, [System.Text.Encoding]::UTF8)
    Write-Host "OK: $Description"
}

Patch-File $file `
    'var app = builder.Build();' `
    'builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();' `
    "Admin Program.cs: ForwardedHeaders para AntiForgery en Azure"

Patch-File $file `
    'app.UseHttpsRedirection();' `
    'app.UseForwardedHeaders();
app.UseHttpsRedirection();' `
    "Admin Program.cs: UseForwardedHeaders antes de HTTPS"

Write-Host "Listo. Haz commit y push."