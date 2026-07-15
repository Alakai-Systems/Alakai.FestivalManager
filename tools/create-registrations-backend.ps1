# Fix-TerraformModule-RunFromPackage.ps1
#
# Anade dos ajustes al modulo reutilizable de Terraform (infrastructure/modules/client/main.tf)
# para que cualquier cliente nuevo (empezando por La Jam) nazca sin los problemas
# que hemos arreglado hoy a mano en Swim Out:
#
#   1. WEBSITE_RUN_FROM_PACKAGE = 1 (en API y Admin)
#      Evita deploys "sucios" donde Kudu fusiona el zip nuevo con archivos viejos
#      en vez de sustituir todo atomicamente. Causa raiz del MissingMethodException
#      de hoy (DLLs fosiles de un paquete NuGet ya eliminado del codigo).
#
#   2. DataProtection__KeyRingPath = /home/DataProtection-Keys (en Admin)
#      Persistencia de las claves de cifrado entre reinicios del contenedor Linux.
#
# NOTA IMPORTANTE: este cambio en el modulo NO afecta a los recursos de Swim Out
# ya existentes (esos se crearon manualmente desde el portal, no via este modulo,
# y siguen pendientes de "terraform import"). Este fix aplica automaticamente la
# proxima vez que se cree un cliente desde cero con `terraform apply` — es decir,
# La Jam.
#
# Ejecutar desde la raiz del repo.

$ErrorActionPreference = "Stop"
$modulePath = "infrastructure/modules/client/main.tf"

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

# --- Patch 1: API app_settings ---
$results += Patch-File -Path $modulePath -Description "Anadir WEBSITE_RUN_FROM_PACKAGE al API" -OldString @'
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"      = "Production"
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
'@ -NewString @'
  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"    = "1"
    "ASPNETCORE_ENVIRONMENT"      = "Production"
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
'@

# --- Patch 2: Admin app_settings ---
$results += Patch-File -Path $modulePath -Description "Anadir WEBSITE_RUN_FROM_PACKAGE y DataProtection__KeyRingPath al Admin" -OldString @'
  app_settings = {
    "ASPNETCORE_ENVIRONMENT"  = "Production"
    "ApiSettings__BaseUrl"    = "https://app-${local.prefix}-api.azurewebsites.net/"
    "ExternalAuth__GoogleClientId" = var.google_client_id
  }
'@ -NewString @'
  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "ASPNETCORE_ENVIRONMENT"         = "Production"
    "ApiSettings__BaseUrl"           = "https://app-${local.prefix}-api.azurewebsites.net/"
    "ExternalAuth__GoogleClientId"   = var.google_client_id
    "DataProtection__KeyRingPath"    = "/home/DataProtection-Keys"
  }
'@

if ($results -contains $false) {
    Write-Host "`nAlgun patch no se pudo aplicar. Pega el contenido actual de main.tf para ajustar el anchor." -ForegroundColor Red
    exit 1
}

Write-Host "`nModulo Terraform actualizado." -ForegroundColor Green
Write-Host "Cuando lances 'terraform apply' para La Jam, nacera ya con WEBSITE_RUN_FROM_PACKAGE y DataProtection__KeyRingPath configurados." -ForegroundColor Cyan
Write-Host "Para Swim Out, este cambio quedara reflejado en el state cuando hagas el 'terraform import' pendiente de los recursos existentes." -ForegroundColor Yellow