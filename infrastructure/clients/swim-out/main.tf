module "swimout" {
  source = "../../modules/client"

  client_name     = "swimout"
  location        = "East US"
  subscription_id = var.subscription_id

  app_service_sku = "B1"
  sql_sku         = "Basic"

  sql_admin_username   = var.sql_admin_username
  sql_admin_password   = var.sql_admin_password
  jwt_secret_key       = var.jwt_secret_key
  redsys_merchant_code = var.redsys_merchant_code
  redsys_terminal      = var.redsys_terminal
  redsys_secret_key    = var.redsys_secret_key
  redsys_merchant_name = var.redsys_merchant_name
  email_host           = var.email_host
  email_port           = var.email_port
  email_username       = var.email_username
  email_password       = var.email_password
  email_from           = var.email_from
  email_from_name      = var.email_from_name
  google_client_id     = var.google_client_id
}

output "api_url"   { value = module.swimout.api_url }
output "admin_url" { value = module.swimout.admin_url }