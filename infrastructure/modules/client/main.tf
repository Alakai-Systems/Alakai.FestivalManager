terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

provider "azurerm" {
  subscription_id = var.subscription_id
  tenant_id       = var.tenant_id
  features {}
}

locals {
  prefix = "alakai-${var.client_name}"
  tags = {
    client  = var.client_name
    project = "alakai-festivalmanager"
  }
  api_hostname   = var.api_hostname_override != "" ? var.api_hostname_override : "app-${local.prefix}-api.azurewebsites.net"
  admin_hostname = var.admin_hostname_override != "" ? var.admin_hostname_override : "app-${local.prefix}-admin.azurewebsites.net"
}

# ── Resource Group ─────────────────────────────────────────────────────────────
resource "azurerm_resource_group" "rg" {
  name     = "rg-${local.prefix}"
  location = var.location
  tags     = local.tags

  lifecycle {
    ignore_changes = [location]
  }
}

# ── App Service Plan (shared by API + Admin) ───────────────────────────────────
resource "azurerm_service_plan" "plan" {
  name                = "plan-${local.prefix}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
  tags                = local.tags

  lifecycle {
    ignore_changes = [name]
  }
}

# ── SQL Server ─────────────────────────────────────────────────────────────────
resource "azurerm_mssql_server" "sql" {
  name                         = "sql-${local.prefix}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  tags                         = local.tags

  lifecycle {
    ignore_changes = [name]
  }
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"

  lifecycle {
    ignore_changes = [name]
  }
}

# ── SQL Database ───────────────────────────────────────────────────────────────
resource "azurerm_mssql_database" "db" {
  name                 = "db-${local.prefix}"
  server_id            = azurerm_mssql_server.sql.id
  sku_name             = var.sql_sku
  storage_account_type = "Local"
  geo_backup_enabled   = false
  tags                 = local.tags
}

# ── API App Service ────────────────────────────────────────────────────────────
resource "azurerm_linux_web_app" "api" {
  name                                     = "app-${local.prefix}-api"
  resource_group_name                      = azurerm_resource_group.rg.name
  location                                 = var.location
  service_plan_id                          = azurerm_service_plan.plan.id
  tags                                     = local.tags
  https_only                               = true
  ftp_publish_basic_authentication_enabled = false
  webdeploy_publish_basic_authentication_enabled = false

  site_config {
    always_on = true
    application_stack {
      dotnet_version = "9.0"
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"    = "1"
    "ASPNETCORE_ENVIRONMENT"      = "Production"
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${azurerm_mssql_server.sql.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.db.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    "Jwt__Issuer"                 = "AlakaiFestivalManager"
    "Jwt__Audience"               = "AlakaiFestivalManager"
    "Jwt__SecretKey"              = var.jwt_secret_key
    "Jwt__ExpirationMinutes"      = "120"
    "Jwt__RefreshTokenExpirationDays" = "30"
    "Redsys__MerchantCode"        = var.redsys_merchant_code
    "Redsys__Terminal"            = var.redsys_terminal
    "Redsys__SecretKey"           = var.redsys_secret_key
    "Redsys__MerchantName"        = var.redsys_merchant_name
    "Redsys__PaymentUrl"          = "https://sis.redsys.es/sis/realizarPago"
    "Redsys__NotificationUrl"     = "https://${local.api_hostname}/api/payments/redsys/notification"
    "Redsys__UrlOk"               = "https://${local.admin_hostname}/user-panel/dashboard?payment=ok"
    "Redsys__UrlKo"               = "https://${local.admin_hostname}/user-panel/dashboard?payment=ko"
    "Email__Host"                 = var.email_host
    "Email__Port"                 = tostring(var.email_port)
    "Email__UserName"             = var.email_username
    "Email__Password"             = var.email_password
    "Email__FromEmail"            = var.email_from
    "Email__FromName"             = var.email_from_name
    "Email__UseSSL"               = "true"
    "ApplicationUrls__PortalUrl"  = "https://${local.admin_hostname}/user-panel"
    "FileStorage__RootPath"       = "wwwroot/uploads/email-images"
    "FileStorage__PublicBaseUrl"  = "https://${local.api_hostname}/uploads/email-images"
    "ExternalAuth__Google__ClientId"          = var.google_client_id
    "GoogleAnalytics__CredentialsJson"        = var.google_analytics_credentials_json
    "CORS_ORIGINS"                = "https://${local.admin_hostname}"
  }

  logs {
    application_logs {
      file_system_level = "Warning"
    }
    http_logs {
      file_system {
        retention_in_days = 7
        retention_in_mb   = 35
      }
    }
  }
}

# ── Admin App Service ──────────────────────────────────────────────────────────
resource "azurerm_linux_web_app" "admin" {
  name                                     = "app-${local.prefix}-admin"
  resource_group_name                      = azurerm_resource_group.rg.name
  location                                 = var.location
  service_plan_id                          = azurerm_service_plan.plan.id
  tags                                     = local.tags
  https_only                               = true
  ftp_publish_basic_authentication_enabled = false
  webdeploy_publish_basic_authentication_enabled = false

  site_config {
    always_on           = true
    websockets_enabled  = true
    application_stack {
      dotnet_version = "9.0"
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = "1"
    "ASPNETCORE_ENVIRONMENT"         = "Production"
    "ApiSettings__BaseUrl"           = "https://${local.api_hostname}/"
    "ExternalAuth__GoogleClientId"   = var.google_client_id
    "DataProtection__KeyRingPath"    = "/home/DataProtection-Keys"
  }
}