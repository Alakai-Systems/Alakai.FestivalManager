# Fix-Step43-CompetitionCancelEmailOrder.ps1
# BUG REAL: al cancelar una inscripcion a competicion, se borraba fisicamente
# la fila (_context.CompetitionEntries.Remove) ANTES de enviar el email de
# cancelacion. Como {{CompetitionName}} se busca en ese momento via
# GetByRegistrationIdAsync, la fila ya no existia y la variable llegaba
# siempre vacia. Se invierte el orden: enviar el email primero, borrar despues.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$path = "Alakai.FestivalManager.Application/Features/CompetitionEntries/Services/CompetitionEntryService.cs"

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

$result = Patch-File -Path $path -Description "Enviar el email antes de borrar la inscripcion a competicion" -OldString @'
        CompetitionEntryDto existingEntryDto = await _getCompetitionEntryByIdHandler.HandleAsync(new GetCompetitionEntryByIdQuery { Id = id }, cancellationToken);

        await _deleteCompetitionEntryHandler.HandleAsync(new DeleteCompetitionEntryCommand { Id = id }, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryCancelled, existingEntryDto.RegistrationId, cancellationToken);
'@ -NewString @'
        CompetitionEntryDto existingEntryDto = await _getCompetitionEntryByIdHandler.HandleAsync(new GetCompetitionEntryByIdQuery { Id = id }, cancellationToken);

        await _emailNotificationService.CreateAndSendEmailAsync(EmailTemplateKey.CompetitionEntryCancelled, existingEntryDto.RegistrationId, cancellationToken);

        await _deleteCompetitionEntryHandler.HandleAsync(new DeleteCompetitionEntryCommand { Id = id }, cancellationToken);
'@

if (-not $result) {
    Write-Host "`nNo se pudo aplicar. Pega el contenido actual de DeleteAsync en CompetitionEntryService.cs." -ForegroundColor Red
    exit 1
}

Write-Host "`nCorregido. dotnet build para confirmar." -ForegroundColor Green
Write-Host "El email de cancelacion ahora se envia mientras la inscripcion todavia existe, asi CompetitionName si se rellena." -ForegroundColor Green