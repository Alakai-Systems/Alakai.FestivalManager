<#
    Fix-UsersCsvImportEncodingAndLimit.ps1
    -----------------------------------------
    Sigue a Fix-UsersCsvImport.ps1 (aplicalo DESPUES de ese).

    Corrige los dos problemas que te ha dado el CSV real de 2343 filas:

    1) Limite de filas demasiado bajo: el validador solo permitia 2000 filas
       por archivo, y tu CSV tiene 2343. Lo sube a 20000.

       (El "Exception User-Unhandled" que te salta en Visual Studio es el
       debugger parando en la ValidationException de FluentValidation --
       eso ya lo haria igual con Create/Update de un solo usuario, es el
       comportamiento normal de esa libreria. No es un fallo nuevo del
       import: simplemente antes no tenias ningun caso que disparase esa
       regla. Puedes darle a Continuar (F5) y el backend responde igual
       con el mensaje de error, que es lo que ves en el banner rojo
       "Validation failed." del modal.)

    2) Acentos rotos (Trist�n, L�pez...): el CSV no esta en UTF-8 -- es
       lo habitual en CSV exportados con Excel en Windows, que por
       defecto usan Windows-1252/ISO-8859-1. El codigo forzaba UTF-8 al
       leer el archivo, así que esos caracteres se perdian. Ahora se
       detecta: si el archivo no es UTF-8 valido, se decodifica como
       Latin1 (cubre bien las tildes, enes y signos del espanol), sin
       anadir ningun paquete nuevo.

    Uso:
        cd Alakai.FestivalManager          # raiz del repo (donde esta el .sln)
        pwsh ./Fix-UsersCsvImportEncodingAndLimit.ps1

    Idempotente y todo-o-nada: si algun anchor no encaja porque el archivo
    local difiere de lo esperado (p.ej. porque aun no has aplicado
    Fix-UsersCsvImport.ps1), no escribe nada y lista el problema.
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Alakai.FestivalManager.sln')) {
    Write-Error "No se encuentra Alakai.FestivalManager.sln en el directorio actual. Ejecuta este script desde la raiz del repo (Alakai.FestivalManager/)."
    exit 1
}

# ----------------------------------------------------------------------------
# Helpers (identicos a los scripts anteriores)
# ----------------------------------------------------------------------------

function Get-NormalizedContent {
    param([string]$Path)

    $raw = [System.IO.File]::ReadAllText($Path)
    $usesCrlf = $raw.Contains("`r`n")
    $normalized = $raw.Replace("`r`n", "`n")

    return [PSCustomObject]@{
        Raw        = $raw
        Normalized = $normalized
        UsesCrlf   = $usesCrlf
    }
}

function Set-NormalizedContent {
    param(
        [string]$Path,
        [string]$NormalizedContent,
        [bool]$UsesCrlf
    )

    $final = if ($UsesCrlf) { $NormalizedContent.Replace("`n", "`r`n") } else { $NormalizedContent }
    [System.IO.File]::WriteAllText($Path, $final, [System.Text.UTF8Encoding]::new($false))
}

function Convert-ToLf {
    param([string]$Text)
    return $Text.Replace("`r`n", "`n")
}

$script:PlanErrors = @()
$script:Plan = @()

function Add-PatchOperation {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Anchor,
        [Parameter(Mandatory)][string]$Replacement,
        [Parameter(Mandatory)][string]$Description
    )

    $script:Plan += [PSCustomObject]@{
        Type        = 'Patch'
        Path        = $Path
        Anchor      = Convert-ToLf $Anchor
        Replacement = Convert-ToLf $Replacement
        Description = $Description
    }
}

function Test-PatchOperation {
    param($Op)

    if (-not (Test-Path -LiteralPath $Op.Path)) {
        return "No existe el archivo: $($Op.Path)"
    }

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 1) {
        return $null
    }

    if ($anchorCount -eq 0) {
        $replacementCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Replacement))).Count
        if ($replacementCount -ge 1) {
            return $null  # ya aplicado -> idempotente
        }
        return "Anchor no encontrado en $($Op.Path) (el archivo local no coincide con lo esperado -- lo mas probable es que aun no hayas aplicado Fix-UsersCsvImport.ps1). Descripcion: $($Op.Description)"
    }

    return "Anchor encontrado $anchorCount veces en $($Op.Path) (deberia ser unico). Descripcion: $($Op.Description)"
}

function Invoke-PatchOperation {
    param($Op)

    $file = Get-NormalizedContent -Path $Op.Path
    $anchorCount = ([regex]::Matches($file.Normalized, [regex]::Escape($Op.Anchor))).Count

    if ($anchorCount -eq 0) {
        Write-Host "  = ya aplicado: $($Op.Path) -- $($Op.Description)" -ForegroundColor DarkGray
        return
    }

    $newNormalized = $file.Normalized.Replace($Op.Anchor, $Op.Replacement)
    Set-NormalizedContent -Path $Op.Path -NormalizedContent $newNormalized -UsesCrlf $file.UsesCrlf

    Write-Host "  + patched: $($Op.Path) -- $($Op.Description)" -ForegroundColor Green
}

# ============================================================================
# 1) Validator: sube el limite de 2000 a 20000 filas
# ============================================================================
Add-PatchOperation -Path 'Alakai.FestivalManager.Application/Features/Users/Validators/BulkImportUsersCommandValidator.cs' `
    -Description 'BulkImportUsersCommandValidator: limite 2000 -> 20000 filas' `
    -Anchor @'
        RuleFor(command => command.Rows.Count)
            .LessThanOrEqualTo(2000)
            .WithMessage("A maximum of 2000 rows can be imported in a single file.");
    }
}
'@ `
    -Replacement @'
        RuleFor(command => command.Rows.Count)
            .LessThanOrEqualTo(20000)
            .WithMessage("A maximum of 20000 rows can be imported in a single file.");
    }
}
'@

# ============================================================================
# 2) Users.razor: lee el CSV como bytes en vez de forzar UTF-8
# ============================================================================
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'Users.razor: OnImportFileSelected lee bytes en vez de forzar UTF-8' `
    -Anchor @'
            using Stream stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using StreamReader reader = new(stream, Encoding.UTF8);
            string content = await reader.ReadToEndAsync();
'@ `
    -Replacement @'
            using Stream stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            using MemoryStream buffer = new();
            await stream.CopyToAsync(buffer);
            string content = DecodeCsvBytes(buffer.ToArray());
'@

# ----------------------------------------------------------------------------
# Users.razor: nuevo metodo DecodeCsvBytes (UTF-8 estricto, con fallback a
# Latin1 si el archivo no es UTF-8 valido -- caso tipico de Excel/Windows)
# ----------------------------------------------------------------------------
Add-PatchOperation -Path 'Alakai.FestivalManager.Admin/Components/Pages/Users.razor' `
    -Description 'Users.razor: metodo DecodeCsvBytes' `
    -Anchor @'
        catch (Exception ex)
        {
            importError = $"The file could not be read: {ex.Message}";
        }
    }

    private string GetSampleValue(int columnIndex)
'@ `
    -Replacement @'
        catch (Exception ex)
        {
            importError = $"The file could not be read: {ex.Message}";
        }
    }

    private static string DecodeCsvBytes(byte[] bytes)
    {
        // UTF-8 con BOM (p.ej. "UTF-8 con BOM" al exportar desde Excel/Sheets).
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        }

        // Sin BOM: probamos UTF-8 estricto. Si los bytes no son UTF-8 valido -- habitual en
        // CSV exportados desde Excel en Windows, que por defecto usan Windows-1252/ISO-8859-1
        // -- caemos a Latin1, que decodifica bien las tildes, enes y signos del espanol sin
        // depender de paquetes adicionales (Windows-1252 no viene incluido en .NET fuera de
        // Windows; Latin1 cubre el mismo rango de caracteres que de verdad se usan aqui).
        try
        {
            UTF8Encoding strictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            return strictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(bytes);
        }
    }

    private string GetSampleValue(int columnIndex)
'@

# ============================================================================
# EJECUCION: validar TODO primero (todo o nada), luego aplicar
# ============================================================================

Write-Host ""
Write-Host "Validando $($script:Plan.Count) cambios contra los archivos locales..." -ForegroundColor Cyan

foreach ($op in $script:Plan) {
    $err = Test-PatchOperation -Op $op
    if ($err) {
        $script:PlanErrors += $err
    }
}

if ($script:PlanErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "NO se ha escrito nada. $($script:PlanErrors.Count) problema(s) encontrados:" -ForegroundColor Red
    foreach ($e in $script:PlanErrors) {
        Write-Host "  - $e" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Lo mas probable es que aun no hayas aplicado Fix-UsersCsvImport.ps1, o que" -ForegroundColor Yellow
    Write-Host "algun archivo se haya editado desde entonces." -ForegroundColor Yellow
    Write-Host ""
    exit 1
}

Write-Host "Todo valida OK. Aplicando cambios..." -ForegroundColor Cyan
Write-Host ""

foreach ($op in $script:Plan) {
    Invoke-PatchOperation -Op $op
}

Write-Host ""
Write-Host "Listo. No hace falta migracion ni paquete nuevo." -ForegroundColor Cyan
Write-Host "  1) dotnet build" -ForegroundColor White
Write-Host "  2) Vuelve a subir el mismo CSV de 2343 filas: ya no deberia saltar el" -ForegroundColor White
Write-Host "     limite de filas, y los nombres con tildes (Tristan, Lopez...) deberian" -ForegroundColor White
Write-Host "     verse bien en la vista previa." -ForegroundColor White
Write-Host ""