# 💰 PetHealth Pro - Cloud Cost Optimization Report

## 1. Optimized Resource Tiers

- **Compute**: Using Azure App Service (Linux B1 Tier) - Cost-effective hosting at ~$13/month.
- **Database**: Using Azure SQL (Basic Tier - 5 DTU) - Optimized for predictable workloads at ~$5/month.
- **Cache**: Azure Redis Basic (C0) - Selected for optimal price/performance ratio.

## 2. Cost Saving Strategies

- **Auto-scaling (HPA)**: Kubernetes only scales up pods during high traffic, saving idle compute costs.
- **Storage Lifecycle**: Log retention set to 7 days to minimize disk consumption.
- **Resource Tagging**: Implemented automated tagging to identify and eliminate orphaned resources during audits.
