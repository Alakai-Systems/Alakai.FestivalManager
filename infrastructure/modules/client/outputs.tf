output "api_url" {
  value = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "admin_url" {
  value = "https://${azurerm_linux_web_app.admin.default_hostname}"
}

output "sql_server_fqdn" {
  value = azurerm_mssql_server.sql.fully_qualified_domain_name
}

output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}