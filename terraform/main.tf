# --- 1. GOVERNANCE & PRIMARY RESOURCE GROUP ---
resource "azurerm_resource_group" "rg" {
  name     = "PetHealth-${var.environment}-RG"
  location = var.location

  # Enterprise Governance Tags
  tags = {
    Project                = "PetHealthAPI"
    Environment            = var.environment
    Owner                  = "Chandrasoodan"
    Compliance             = "Enterprise-Standard"
    CostCenter             = "Internal-R&D"
    ManagedBy              = "Terraform-IaC"
    LastArchitectureReview = "2026-07-03"
    Role                   = "Primary"
  }
}

# --- 2. NETWORKING, CACHE & MONITORING ---

# Virtual Network
resource "azurerm_virtual_network" "vnet" {
  name                = "pethealth-vnet-${var.environment}"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

# Redis Cache
resource "azurerm_redis_cache" "redis" {
  name                 = "pethealth-cache-${lower(var.environment)}"
  location             = azurerm_resource_group.rg.location
  resource_group_name  = azurerm_resource_group.rg.name
  capacity             = 0
  family               = "C"
  sku_name             = "Basic"
  non_ssl_port_enabled = false
}

# Application Insights (Monitoring)
resource "azurerm_application_insights" "insights" {
  name                = "pethealth-ai-${lower(var.environment)}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

# --- 3. PRIMARY REGION COMPUTE & DB (East US) ---

# SQL Server Primary
resource "azurerm_mssql_server" "sql" {
  name                         = "pethealth-sql-primary-${lower(var.environment)}"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = "petadmin"
  administrator_login_password = "ComplexPassword123!"
}

# SQL Database Primary
resource "azurerm_mssql_database" "db" {
  name      = "PetPulseDB"
  server_id = azurerm_mssql_server.sql.id
  sku_name  = "Basic"

  short_term_retention_policy {
    retention_days = 7
  }

  long_term_retention_policy {
    weekly_retention  = "P4W"  # 4 weeks
    monthly_retention = "P12M" # 12 months
  }
}

# App Service Plan Primary
resource "azurerm_service_plan" "plan" {
  name                = "pethealth-plan-primary"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# Web App Primary
resource "azurerm_linux_web_app" "app" {
  name                = "pethealth-api-primary-${lower(var.environment)}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.plan.id
  site_config {
    application_stack {
      docker_image_name = "pethealthapi:latest"
    }
  }
}

# --- 4. DISASTER RECOVERY (DR) SITE - SECONDARY REGION  ---

# Secondary Resource Group (West US)
resource "azurerm_resource_group" "rg_dr" {
  name     = "PetHealth-${var.environment}-DR-RG"
  location = "West US"
  tags     = { Environment = var.environment, Role = "Secondary" }
}

# Secondary SQL Server (DR)
resource "azurerm_mssql_server" "sql_dr" {
  name                         = "pethealth-sql-dr-${lower(var.environment)}"
  resource_group_name          = azurerm_resource_group.rg_dr.name
  location                     = azurerm_resource_group.rg_dr.location
  version                      = "12.0"
  administrator_login          = "petadmin"
  administrator_login_password = "ComplexPassword123!"
}

# DB Geo-Replication / Failover Group
resource "azurerm_mssql_failover_group" "sql_failover" {
  name      = "pethealth-db-failover"
  server_id = azurerm_mssql_server.sql.id
  databases = [azurerm_mssql_database.db.id]

  partner_server {
    id = azurerm_mssql_server.sql_dr.id
  }

  read_write_endpoint_failover_policy {
    mode          = "Automatic"
    grace_minutes = 60
  }
}

# Secondary App Service Plan (DR)
resource "azurerm_service_plan" "plan_dr" {
  name                = "pethealth-plan-dr"
  resource_group_name = azurerm_resource_group.rg_dr.name
  location            = azurerm_resource_group.rg_dr.location
  os_type             = "Linux"
  sku_name            = "B1"
}

# Secondary Web App (DR)
resource "azurerm_linux_web_app" "app_dr" {
  name                = "pethealth-api-dr-${lower(var.environment)}"
  resource_group_name = azurerm_resource_group.rg_dr.name
  location            = azurerm_resource_group.rg_dr.location
  service_plan_id     = azurerm_service_plan.plan_dr.id
  site_config {
    application_stack {
      docker_image_name = "pethealthapi:latest"
    }
  }
}

# --- 5. GLOBAL TRAFFIC MANAGER (Load Balancing & Failover) ---
resource "azurerm_traffic_manager_profile" "tm" {
  name                   = "pethealth-global-router"
  resource_group_name    = azurerm_resource_group.rg.name
  traffic_routing_method = "Priority"

  dns_config {
    relative_name = "pethealthapi-global-dns"
    ttl           = 30
  }

  monitor_config {
    protocol = "HTTPS"
    port     = 443
    path     = "/health"
  }
}
#  Geo-Redundant Storage (GRS) for Data Replication
resource "azurerm_storage_account" "pet_storage" {
  name                = "pethealthstore${lower(var.environment)}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  account_tier        = "Standard"

  # Cross-region replication for storage
  account_replication_type = "GRS" # Geo-Redundant Storage (Auto-syncs to West US)

  tags = { Role = "Primary-Storage" }
}