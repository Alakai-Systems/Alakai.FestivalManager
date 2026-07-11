variable "client_name" {
  description = "Short client identifier, used in resource names (e.g. 'swimout', 'lajam')"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "East US"
}

variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "app_service_sku" {
  description = "App Service plan SKU (B1 = Basic, S1 = Standard)"
  type        = string
  default     = "B1"
}

variable "sql_sku" {
  description = "Azure SQL database SKU"
  type        = string
  default     = "Basic"
}

variable "sql_admin_username" {
  description = "SQL Server admin username"
  type        = string
  default     = "alakaiadmin"
}

variable "sql_admin_password" {
  description = "SQL Server admin password"
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "JWT signing secret (min 32 chars)"
  type        = string
  sensitive   = true
}

variable "redsys_merchant_code" {
  type      = string
  sensitive = true
}

variable "redsys_terminal" {
  type    = string
  default = "1"
}

variable "redsys_secret_key" {
  type      = string
  sensitive = true
}

variable "redsys_merchant_name" {
  type = string
}

variable "email_host" {
  type = string
}

variable "email_port" {
  type    = number
  default = 587
}

variable "email_username" {
  type      = string
  sensitive = true
}

variable "email_password" {
  type      = string
  sensitive = true
}

variable "email_from" {
  type = string
}

variable "email_from_name" {
  type = string
}

variable "google_client_id" {
  type      = string
  sensitive = true
  default   = ""
}