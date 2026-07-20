#  PetHealth Pro - Unified Customer Intelligence Framework

## 1. Unified Customer View (360-Degree View)

To consolidate all data points into a single entity profile for each Pet Owner.

### Data Sources:

- **Identity Layer**: UserID, Email, Subscription Plan (from SQL Server).
- **Behavioral Layer**: Login frequency, Feature usage (from OpenTelemetry/Logs).
- **Domain Layer**: Pet health trends, Vaccination completion (from PetEvents table).
- **Business Layer**: Lifetime Value (LTV), Monthly Recurring Revenue (MRR).

## 2. Intelligence Orchestration Flow

1. **Data Ingestion**: Collecting raw signals from API logs and DB.
2. **Entity Resolution**: Matching behavioral logs with specific User IDs.
3. **Signal Consolidation**: Merging health scores with revenue data.
4. **Action Trigger**: Recommending business actions based on consolidated scores.
