 PetHealth Pro API - Production Handover Document
1. Project Overview
Project Name: PetHealth Pro (Enterprise Edition)
Version: 1.0.0
Primary Owner: Chandrasoodan R
Description: A resilient, high-performance .NET 8 API for distributed pet health management, featuring Kubernetes orchestration and Terraform-based infrastructure.
2. Infrastructure & Deployment
 Cloud Resources (Managed via Terraform)
Resource Group: PetHealth-Prod-RG
Compute: Azure App Service (Linux, B1 Tier)
Database: Azure SQL Database (Basic Tier)
Cache: Azure Redis Cache
 Orchestration (Kubernetes)
Cluster: Docker Desktop / Managed K8s
Key Manifests: Found in /k8s folder.
Autoscaling: HPA configured to scale from 2 to 5 replicas based on 50% CPU load.
3. Security & Access
Auth Provider: JWT Bearer Token.
Roles: Admin (Full access), User (Read/Write pets).
Key Rotation: Refresh Token Rotation implemented for long-running sessions.
Rate Limiting: Enabled via fixed-window policy (10 req / 10 sec).
4. Maintenance & Operations (Runbook)
 Monitoring
Traces: Access Jaeger UI at http://localhost:16686 for request lifecycles.
Metrics: Custom meters for Cache Hit/Miss and Job duration available via OpenTelemetry.
Audit Logs: All sensitive operations (DELETE/POST) are logged with User context in Serilog sinks.
 Backup & Recovery
Database: Daily automated SQL backups with 30-day retention.
State: Terraform state managed via remote backend for drift detection.
5. Performance Standards (SLO/SLI)
Availability Target: 99.9% Uptime.
Latency Target: 95% of GET requests under 500ms.
Error Budget: Maximum 43 minutes of unplanned downtime per month.
6. Known Procedures
To Scale Up: kubectl scale deployment pethealthapi --replicas=5
To Deploy Fix: Push code to main branch; GitHub Actions triggers the automated CI/CD pipeline.
To Check Logs: Inspect /Logs/petapi_log.txt or Azure App Insights.