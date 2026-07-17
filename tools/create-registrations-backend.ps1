# Fix-Step32-RowColorPrecedenceBug.ps1
# BUG REAL (preexistente): falta de parentesis hacia que "isOverdue" solo
# aplicara a la rama de DeferredTenDays, no a la de SplitFiftyFifty (por la
# precedencia de && sobre || en C#). Cualquier reserva con pago 50/50
# parcialmente pagada se pintaba en rojo de inmediato, sin comprobar si de
# verdad estaba vencida - y como el codigo salia con return ahi, nunca
# llegaba a la comprobacion de pareja pendiente (amarillo).
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Admin/Components/Pages/Registrations.razor"

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

$result = Patch-File -Path $path -Description "Anadir parentesis para que isOverdue aplique a ambas ramas" -OldString @'
        if (isOverdue &&
            (registration.PaymentPlan == PaymentPlan.DeferredTenDays && registration.PaymentStatus != PaymentStatus.Paid) ||
            (registration.PaymentPlan == PaymentPlan.SplitFiftyFifty && registration.PaymentStatus == PaymentStatus.PartiallyPaid))
        {
            return "bg-danger/10";
        }
'@ -NewString @'
        if (isOverdue &&
            ((registration.PaymentPlan == PaymentPlan.DeferredTenDays && registration.PaymentStatus != PaymentStatus.Paid) ||
             (registration.PaymentPlan == PaymentPlan.SplitFiftyFifty && registration.PaymentStatus == PaymentStatus.PartiallyPaid)))
        {
            return "bg-danger/10";
        }
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de GetRowClass en Registrations.razor." -ForegroundColor Red
    exit 1
}

Write-Host "`nBug corregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "Ahora el rojo solo aparece cuando de verdad esta vencido, y el amarillo de pareja pendiente vuelve a mostrarse correctamente." -ForegroundColor Green