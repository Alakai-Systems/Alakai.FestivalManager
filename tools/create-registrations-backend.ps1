# fix_antiforgery.ps1
# Ejecutar desde la raiz del repo: .\tools\fix_antiforgery.ps1

$ErrorActionPreference = "Stop"

$file = "Alakai.FestivalManager.Admin\Endpoints\AdminAuthEndpoints.cs"

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
    '                return Results.Redirect("/login?error=invalid");
            }
        }).AllowAnonymous();
        app.MapPost("/account/logout"' `
    '                return Results.Redirect("/login?error=invalid");
            }
        }).AllowAnonymous().DisableAntiforgery();
        app.MapPost("/account/logout"' `
    "AdminAuthEndpoints: DisableAntiforgery en login"

Patch-File $file `
    '            return Results.Redirect("/login");
        }).AllowAnonymous();
    }
}' `
    '            return Results.Redirect("/login");
        }).AllowAnonymous().DisableAntiforgery();
    }
}' `
    "AdminAuthEndpoints: DisableAntiforgery en logout"

Write-Host "Listo. Haz commit y push."