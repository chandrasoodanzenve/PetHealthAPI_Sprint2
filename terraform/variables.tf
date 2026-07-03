variable "environment" {
  description = "Deployment environment (Dev/Prod)"
  type        = string
  default     = "Development"
}

variable "location" {
  default = "East US"
}
variable "sku_name" {
  description = "The SKU for the Web App and Cache"
  type        = string
  default     = "B1"
}