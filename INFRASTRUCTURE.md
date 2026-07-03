# Infrastructure Provisioning Guide (IaC)

## Environments
- **Development**: Managed via `environments/dev.tfvars`
- **Production**: Managed via `environments/prod.tfvars`

## Deployment Steps
1. Navigate to terraform folder.
2. Initialize: `./terraform init`
3. Plan for Dev: `./terraform plan -var-file="environments/dev.tfvars"`
4. Apply: `./terraform apply -var-file="environments/dev.tfvars"`

## State Management
Remote state is stored in Azure Blob Storage for team synchronization.