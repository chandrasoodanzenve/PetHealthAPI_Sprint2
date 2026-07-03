# 1. Resource Group
resource "azurerm_resource_group" "rg" {
  name     = "PetHealth-${var.environment}-RG"
  location = var.location
   # Governance Tags
  tags = {
    Project           = "PetHealthAPI"
    Environment       = var.environment
    Owner             = "Chandrasoodan"
    Compliance        = "Enterprise-Standard"
    CostCenter        = "Internal-R&D"
    ManagedBy         = "Terraform-IaC"
    LastArchitectureReview = "2026-07-03"
  }
}
# 2. Virtual Network (Networking - Task Point 1)
resource "azurerm_virtual_network" "vnet" {
  name                = "pethealth-vnet-${var.environment}"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}
# 3. Redis Cache (Cache - Task Point 1)
resource "azurerm_redis_cache" "redis" {
  name                = "pethealth-cache-${lower(var.environment)}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  non_ssl_port_enabled = false
}
# 4. Application Insights (Monitoring - Task Point 1)
resource "azurerm_application_insights" "insights" {
  name                = "pethealth-ai-${lower(var.environment)}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

# 5. SQL Server & Database (Compute/Database)
resource "azurerm_mssql_server" "sql" {
  name                         = "pethealth-sql-${lower(var.environment)}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = "petadmin"
  administrator_login_password = "ComplexPassword123!"
}

resource "azurerm_mssql_database" "db" {
  name      = "PetPulseDB"
  server_id = azurerm_mssql_server.sql.id
  sku_name  = "Basic"
}

# 6. App Service Plan
resource "azurerm_service_plan" "plan" {
  name                = "pethealth-plan-${var.environment}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# 7. Web App for Containers (Compute)
resource "azurerm_linux_web_app" "app" {
  name                = "pethealth-api-${lower(var.environment)}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.plan.id

  site_config {
    application_stack {
      docker_image_name = "pethealthapi:latest"
    }
  }
}