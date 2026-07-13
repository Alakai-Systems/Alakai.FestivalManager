variable "subscription_id" {
  type = string
}

variable "sql_admin_username" {
  type    = string
  default = "alakaiadmin"
}

variable "sql_admin_password" {
  type      = string
  sensitive = true
}

variable "jwt_secret_key" {
  type      = string
  sensitive = true
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
  type    = string
  default = ""
}

variable "google_analytics_credentials_json" {
  type      = string
  sensitive = true
  default   = ""
}

variable "tenant_id" {
  type = string
}